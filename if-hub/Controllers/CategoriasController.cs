using if_hub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace if_hub.Controllers
{
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
        [AllowAnonymous]
        public async Task<IActionResult> GetCategorias()
        {
            var categorias = await _context.Categorias
                .OrderBy(c => c.Nome)
                .Select(c => new CategoriaViewModel
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    Descricao = c.Descricao
                })
                .ToListAsync();

            return Ok(categorias);
        }

        // GET: api/categorias/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategoria(int id)
        {
            var categoria = await _context.Categorias
                .Where(c => c.Id == id)
                .Select(c => new CategoriaViewModel
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    Descricao = c.Descricao
                })
                .FirstOrDefaultAsync();

            if (categoria == null)
            {
                return NotFound();
            }

            return Ok(categoria);
        }

        // GET: api/categorias/5/topicos
        [HttpGet("{id}/topicos")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTopicosPorCategoria(int id)
        {
            var userId = User.Identity.IsAuthenticated
                ? int.Parse(User.FindFirstValue("UserId"))
                : (int?)null;

            var topicos = await _context.Topicos
                .Where(t => t.CategoriaId == id)
                .Where(t => !t.Excluido)
                .Include(t => t.Usuario)
                .Include(t => t.Categoria)
                .Include(t => t.Respostas)
                .Include(t => t.Curtidas)
                .OrderByDescending(t => t.DataCriacao)
                .Select(t => new TopicListItemViewModel
                {
                    Id = t.Id,
                    Titulo = t.Titulo,
                    UsuarioNome = t.Usuario != null ? t.Usuario.Nome : "Usuário Deletado",
                    CategoriaNome = t.Categoria != null ? t.Categoria.Nome : "Sem Categoria",
                    TotalRespostas = t.Respostas.Count(),
                    TotalCurtidas = t.Curtidas.Count(),
                    UsuarioCurtiu = userId.HasValue && t.Curtidas.Any(c => c.UsuarioId == userId.Value)
                })
                .ToListAsync();

            return Ok(topicos);
        }
    }
}