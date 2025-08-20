namespace if_hub.Controllers
{
    using if_hub.ViewModels;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using if_hub.Entities;

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "3")]
    public class RolesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RolesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/roles
        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _context.Roles
                .Select(r => new RoleViewModel { Id = r.Id, Nome = r.Nome })
                .ToListAsync();

            return Ok(roles);
        }
    }
}
