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
    options.UseSqlite(connectionString, 
        b => b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

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

builder.Services.Configure<GoogleAuthSettings>(builder.Configuration.GetSection("GoogleAuth"));

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
    })
.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["GoogleAuth:ClientId"];
    googleOptions.ClientSecret = builder.Configuration["GoogleAuth:ClientSecret"];
    
    googleOptions.Events = new OAuthEvents
    {
        OnCreatingTicket = async context =>
        {
            var email = context.Identity.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                throw new Exception("Não foi possível obter o e-mail do perfil do Google.");
            }

            if (!email.EndsWith("@aluno.ifsp.edu.br", StringComparison.OrdinalIgnoreCase) &&
                !email.EndsWith("@ifsp.edu.br", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"Acesso restrito a e-mails do IFSP. O e-mail '{email}' não é permitido.");
            }

            var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            var user = await dbContext.Usuarios.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                var name = context.Identity.FindFirst(ClaimTypes.Name)?.Value;
                var newUser = new Usuario
                {
                    Email = email,
                    Nome = name ?? "Novo Usuário",
                    DataCriacao = DateTime.UtcNow,
                    RoleId = 1 
                };
                dbContext.Usuarios.Add(newUser);
                await dbContext.SaveChangesAsync();
                user = newUser;
            }
            
            if (user != null && user.Banido)
            {
                throw new Exception("Esta conta foi banida e não pode mais acessar o sistema.");
            }
            
            if (user != null)
            {
                var identity = (ClaimsIdentity)context.Principal.Identity;
                if (user.Role != null)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, user.Role.Id.ToString()));
                }
                identity.AddClaim(new Claim("UserId", user.Id.ToString()));
            }
        },
        
        OnRemoteFailure = context =>
        {
            var errorMessage = context.Failure?.Message;
            context.HandleResponse();
            context.Response.Redirect($"/login?error={Uri.EscapeDataString(errorMessage ?? "Erro de autenticação externa")}");
            return Task.CompletedTask;
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

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<HttpClientCookieHandler>();


builder.Services.AddHttpClient("ServerAPI", client =>
    {
        client.BaseAddress = new Uri("https://localhost:7029"); 
    })
    .AddHttpMessageHandler<HttpClientCookieHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ServerAPI"));


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
