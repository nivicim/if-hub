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
            var userId = User.Identity.IsAuthenticated
                ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier))
                : (int?)null;

            var topicosDoBanco = await _context.Topicos
                .Include(t => t.Usuario)
                .Include(t => t.Categoria)
                .Include(t => t.Respostas)
                .Include(t => t.Curtidas)
                .OrderByDescending(t => t.DataCriacao)
                .ToListAsync();

            var resultadoFinal = topicosDoBanco.Select(t => new TopicListItemViewModel
            {
                Id = t.Id,
                Titulo = t.Titulo,
                UsuarioNome = t.Usuario != null ? t.Usuario.Nome : "Usuário Deletado",
                CategoriaNome = t.Categoria != null ? t.Categoria.Nome : "Sem Categoria",
                TotalRespostas = t.Respostas.Count(),
                TotalCurtidas = t.Curtidas.Count(),
                UsuarioCurtiu = userId.HasValue && t.Curtidas.Any(c => c.UsuarioId == userId.Value)
            }).ToList();

            return Ok(resultadoFinal);
        }

        // GET: api/topicos/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTopico(int id)
        {
            var userId = User.Identity.IsAuthenticated
                ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier))
                : (int?)null;

            var topico = await _context.Topicos
                .Where(t => t.Id == id)
                .Include(t => t.Usuario)
                .Include(t => t.Categoria)
                .Include(t => t.Respostas).ThenInclude(r => r.Usuario)
                .Include(t => t.Respostas).ThenInclude(r => r.Curtidas)
                .Include(t => t.Curtidas)
                .Select(t => new TopicDetailViewModel
                {
                    Id = t.Id,
                    Titulo = t.Titulo,
                    Conteudo = t.Conteudo,
                    DataCriacao = t.DataCriacao,
                    EditadoEm = t.EditadoEm,
                    UsuarioId = t.UsuarioId,
                    UsuarioNome = t.Usuario.Nome,
                    CategoriaNome = t.Categoria.Nome,
                    TotalCurtidas = t.Curtidas.Count(),
                    UsuarioCurtiu = userId.HasValue && t.Curtidas.Any(c => c.UsuarioId == userId.Value),
                    Respostas = t.Respostas.Select(r => new RespostaViewModel
                    {
                        Id = r.Id,
                        Conteudo = r.Conteudo,
                        DataCriacao = r.DataCriacao,
                        EditadoEm = r.EditadoEm,
                        UsuarioId = r.UsuarioId,
                        UsuarioNome = r.Usuario.Nome,
                        TotalCurtidas = r.Curtidas.Count(),
                        UsuarioCurtiu = userId.HasValue && r.Curtidas.Any(c => c.UsuarioId == userId.Value)
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
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized();
            }
            var userId = int.Parse(userIdString);

            var topico = await _context.Topicos.FindAsync(id);

            if (topico == null)
            {
                return NotFound();
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
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized();
            }
            var userId = int.Parse(userIdString);

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

        // POST: api/topicos/{id}/curtir
        [HttpPost("{id}/curtir")]
        [Authorize]
        public async Task<IActionResult> CurtirTopico(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var topico = await _context.Topicos.FindAsync(id);
            if (topico == null)
            {
                return NotFound("Tópico não encontrado.");
            }

            var curtidaExistente = await _context.Curtidas
                .FirstOrDefaultAsync(c => c.TopicoId == id && c.UsuarioId == userId);

            if (curtidaExistente != null)
            {
                return BadRequest("Você já curtiu este tópico.");
            }

            var novaCurtida = new Curtida
            {
                UsuarioId = userId,
                TopicoId = id,
                Data = DateTime.UtcNow
            };

            _context.Curtidas.Add(novaCurtida);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/topicos/{id}/curtir
        [HttpDelete("{id}/curtir")]
        [Authorize]
        public async Task<IActionResult> DescurtirTopico(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var curtida = await _context.Curtidas
                .FirstOrDefaultAsync(c => c.TopicoId == id && c.UsuarioId == userId);

            if (curtida == null)
            {
                return NotFound("Você ainda não curtiu este tópico para poder descurtir.");
            }

            _context.Curtidas.Remove(curtida);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}