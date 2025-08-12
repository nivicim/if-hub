namespace if_hub.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        
        public CategoriasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/categorias
        [HttpGet]
        public async Task<IActionResult> GetCategorias()
        {            
            var categorias = await _context.Categorias.ToListAsync();

            return Ok(categorias);
        }
    }
}
