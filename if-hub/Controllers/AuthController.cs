using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace Controllers
{
    using if_hub.Entities;
    using if_hub.ViewModels;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity; 
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;



    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<Usuario> _passwordHasher;
        private readonly IConfiguration _configuration;


        public AuthController(ApplicationDbContext context, IPasswordHasher<Usuario> passwordHasher, IConfiguration configuration)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }


        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            // 1. Verificar se o e-mail já está em uso
            if (await _context.Usuarios.AnyAsync(u => u.Email == registerViewModel.Email))
            {
                return BadRequest("Este e-mail já está em uso.");
            }

            // 2. Criar o novo objeto Usuario (ainda sem o hash)
            var novoUsuario = new Usuario
            {
                Nome = registerViewModel.Nome,
                Email = registerViewModel.Email,
                DataCriacao = DateTime.UtcNow,
                RoleId = 1
            };

            // 3. Fazer o hash da senha usando o PasswordHasher injetado
            var senhaHash = _passwordHasher.HashPassword(novoUsuario, registerViewModel.Senha);
            novoUsuario.SenhaHash = senhaHash;

            // 4. Salvar o usuário no banco de dados
            _context.Usuarios.Add(novoUsuario);
            await _context.SaveChangesAsync();

            // 5. Retornar uma resposta de sucesso
            return Ok(new { message = "Usuário registrado com sucesso!" });
        }
        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            // 1. Encontrar o usuário pelo e-mail
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == loginViewModel.Email);

            if (usuario == null)
            {
                return Unauthorized("Credenciais inválidas.");
            }

            // 2. Verificar a senha
            var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.SenhaHash, loginViewModel.Senha);

            if (resultado == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Credenciais inválidas.");
            }

            // 3. Gerar o Token JWT
            var token = GenerateJwtToken(usuario);

            return Ok(new { token });
        }

        private string GenerateJwtToken(Usuario usuario)
        {
            // 1. Obter a chave secreta da configuração
            var jwtKey = _configuration["Jwt:Key"];

            // 2. Validar se a chave existe. Se não, a aplicação não pode gerar tokens seguros.
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("A chave secreta do JWT (Jwt:Key) não está configurada no appsettings.json");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtKey);

            var claims = new ClaimsIdentity(new[]
            {
        new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
        new Claim(ClaimTypes.Email, usuario.Email),
        new Claim(ClaimTypes.Role, usuario.RoleId.ToString()),
        new Claim(ClaimTypes.Name, usuario.Nome)
    });

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddHours(8),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(securityToken);
        }

        // GET: api/auth/profile
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized();
            }

            var usuario = await _context.Usuarios.FindAsync(int.Parse(userIdString));

            if (usuario == null)
            {
                return NotFound("Usuário não encontrado.");
            }


            return Ok(new { id = usuario.Id, nome = usuario.Nome, email = usuario.Email });
        }
    }
}
