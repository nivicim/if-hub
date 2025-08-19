namespace if_hub.Controllers
{
    using if_hub.Entities; 
    using if_hub.ViewModels;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Security.Claims;

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "3")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/admin/users
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Usuarios
                .Include(u => u.Role) 
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    Email = u.Email,
                    RoleNome = u.Role.Nome
                })
                .ToListAsync();

            return Ok(users);
        }

        // PUT: api/admin/users/{userId}/role
        [HttpPut("users/{userId}/role")]
        public async Task<IActionResult> UpdateUserRole(int userId, [FromBody] UpdateUserRoleViewModel model)
        {
            var userToUpdate = await _context.Usuarios.FindAsync(userId);
            if (userToUpdate == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            var roleExists = await _context.Roles.AnyAsync(r => r.Id == model.NewRoleId);
            if (!roleExists)
            {
                return BadRequest("O papel (Role) especificado não existe.");
            }

            var loggedInAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userToUpdate.Id.ToString() == loggedInAdminId)
            {
                return BadRequest("Administradores não podem alterar o próprio papel.");
            }

            userToUpdate.RoleId = model.NewRoleId;
            await _context.SaveChangesAsync();

            return NoContent(); 
        }
    }
}
