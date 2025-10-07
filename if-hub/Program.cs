using System.Security.Claims;
using if_hub.Auth;
using if_hub.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using if_hub.Services;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Components.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(); 

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<ServerAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
builder.Services.AddRazorPages();

builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7029") });

var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("A chave secreta do JWT (Jwt:Key) n�o est� configurada no appsettings.json");
}

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme; // Define Google como desafio padrão
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
    })
.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    
    googleOptions.Events = new OAuthEvents
    {
        OnCreatingTicket = async context =>
        {
            var email = context.Identity.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                context.Fail("Não foi possível obter o e-mail do perfil.");
                return;
            }

            if (!email.EndsWith("@aluno.ifsp.edu.br", StringComparison.OrdinalIgnoreCase) &&
                !email.EndsWith("@ifsp.edu.br", StringComparison.OrdinalIgnoreCase))
            {
                context.Fail($"Acesso restrito a e-mails do IFSP. E-mail '{email}' não é permitido.");
                return;
            }

            var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            
            var user = await dbContext.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                var name = context.Identity.FindFirst(ClaimTypes.Name)?.Value;
                var newUser = new Usuario
                {
                    Email = email,
                    Nome = name ?? "Novo Usuário",
                    DataCriacao = DateTime.UtcNow,
                    RoleId = 1 // Role padrão de Aluno
                };
                dbContext.Usuarios.Add(newUser);
                await dbContext.SaveChangesAsync();
                user = newUser; 
            }
            
            if (user != null)
            {
                var identity = (ClaimsIdentity)context.Principal.Identity;
                
                // Busca a Role (permissão) do usuário no banco de dados
                var role = await dbContext.Roles.FindAsync(user.RoleId);
                if (role != null)
                {
                    // Adiciona a claim de Role, que o [Authorize(Roles = "...")] usa
                    identity.AddClaim(new Claim(ClaimTypes.Role, role.Id.ToString()));
                }

                // Adiciona uma claim customizada com o nosso ID interno do usuário.
                identity.AddClaim(new Claim("UserId", user.Id.ToString()));
            }
        }
    };
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 100 * 1024 * 1024;
});


builder.Services.AddServerSideBlazor()
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 100 * 1024 * 1024;

        options.ClientTimeoutInterval = TimeSpan.FromMinutes(5);

        options.KeepAliveInterval = TimeSpan.FromSeconds(30);
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); 
app.UseAntiforgery();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot/uploads")),
    RequestPath = "/uploads"
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapRazorComponents<if_hub.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
