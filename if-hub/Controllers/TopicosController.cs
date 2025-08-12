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
    public class TopicosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TopicosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/topicos
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetTopicos()
        {
            var topicosDoBanco = await _context.Topicos
                .Include(t => t.Usuario)
                .Include(t => t.Categoria)
                .Include(t => t.Respostas) 
                .OrderByDescending(t => t.DataCriacao)
                .ToListAsync(); 

            var resultadoFinal = topicosDoBanco.Select(t => new
            {
                t.Id,
                t.Titulo,
                t.Conteudo,
                t.DataCriacao,
                UsuarioNome = t.Usuario != null ? t.Usuario.Nome : "Usuário Deletado",
                CategoriaNome = t.Categoria != null ? t.Categoria.Nome : "Sem Categoria",
                TotalRespostas = t.Respostas.Count()
            }).ToList();

            return Ok(resultadoFinal);
        }

        // GET: api/topicos/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTopico(int id)
        {
            var topico = await _context.Topicos
                .Where(t => t.Id == id) 
                .Include(t => t.Usuario)
                .Include(t => t.Categoria)
                .Include(t => t.Respostas)
                    .ThenInclude(r => r.Usuario) 
                .Select(t => new TopicDetailViewModel
                {
                    Id = t.Id,
                    Titulo = t.Titulo,
                    Conteudo = t.Conteudo,
                    DataCriacao = t.DataCriacao,
                    UsuarioId = t.UsuarioId,
                    UsuarioNome = t.Usuario.Nome,
                    CategoriaNome = t.Categoria.Nome,
                    Respostas = t.Respostas.Select(r => new RespostaViewModel
                    {
                        Id = r.Id,
                        Conteudo = r.Conteudo,
                        DataCriacao = r.DataCriacao,
                        EditadoEm = r.EditadoEm,
                        UsuarioId = r.UsuarioId,
                        UsuarioNome = r.Usuario.Nome
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (topico == null)
            {
                return NotFound();
            }
            return Ok(topico);
        }

        // POST: api/topicos
        [HttpPost]
        [Authorize] 
        public async Task<IActionResult> CreateTopico(CreateTopicViewModel topicViewModel)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized();
            }
            var userId = int.Parse(userIdString);

            var novoTopico = new Topico
            {
                Titulo = topicViewModel.Titulo,
                Conteudo = topicViewModel.Conteudo,
                DataCriacao = DateTime.UtcNow,
                CategoriaId = topicViewModel.CategoriaId,
                UsuarioId = userId
            };

            _context.Topicos.Add(novoTopico);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTopico), new { id = novoTopico.Id }, novoTopico);
        }

        // DELETE: api/topicos/x
        [HttpDelete("{id}")]
        [Authorize] 
        public async Task<IActionResult> DeleteTopico(int id)
        {
            // Obter o ID do usuário logado a partir do token
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized();
            }
            var userId = int.Parse(userIdString);

            // Encontrar o tópico no banco de dados
            var topico = await _context.Topicos.FindAsync(id);

            if (topico == null)
            {
                return NotFound(); // Tópico não existe
            }

            if (topico.UsuarioId != userId && userRole != "2" && userRole != "3")
            {
                return Forbid();
            }

            _context.Topicos.Remove(topico);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        // PUT: api/topicos/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateTopico(int id, UpdateTopicViewModel topicViewModel)
        {
            // Obter o ID do usuário logado a partir do token
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized();
            }
            var userId = int.Parse(userIdString);

            // Encontrar o tópico no banco de dados
            var topico = await _context.Topicos.FindAsync(id);

            if (topico == null)
            {
                return NotFound();
            }

            if (topico.UsuarioId != userId && userRole != "2" && userRole != "3")
            {
                return Forbid();
            }

            topico.Titulo = topicViewModel.Titulo;
            topico.Conteudo = topicViewModel.Conteudo;
            topico.EditadoEm = DateTime.UtcNow; 

            await _context.SaveChangesAsync();

            return NoContent(); 
        }
    }
}
