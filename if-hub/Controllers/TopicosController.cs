namespace if_hub.Controllers
{
    using if_hub.Entities;
    using if_hub.Services;
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

        private readonly IFileStorageService _fileStorageService;

        public TopicosController(ApplicationDbContext context, IFileStorageService fileStorageService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
        }

        // GET: api/topicos
// Em Controllers/TopicosController.cs

// GET: api/topicos
[HttpGet]
[AllowAnonymous]
public async Task<IActionResult> GetTopicos(
    [FromQuery] string sortBy = "recentes", 
    [FromQuery] int page = 1, 
    [FromQuery] int pageSize = 10)
{
    var userId = User.Identity.IsAuthenticated ? int.Parse(User.FindFirstValue("UserId")) : (int?)null;

    var query = _context.Topicos
        .Where(t => !t.Excluido)
        .Include(t => t.Usuario)
        .Include(t => t.Categoria)
        .Include(t => t.Respostas)
        .Include(t => t.Curtidas)
        .AsQueryable();

    // Aplica a ordenação baseada no parâmetro 'sortBy'
    switch (sortBy.ToLower())
    {
        case "curtidas":
            query = query.OrderByDescending(t => t.Curtidas.Count());
            break;
        case "respostas":
            query = query.OrderByDescending(t => t.Respostas.Count());
            break;
        case "emalta":
            query = query.OrderByDescending(t => (t.Curtidas.Count() * 1) + (t.Respostas.Count() * 2))
                         .ThenByDescending(t => t.DataCriacao);
            break;
        case "recentes":
        default:
            query = query.OrderByDescending(t => t.DataCriacao);
            break;
    }

    var totalItems = await query.CountAsync();
    var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

    var pagedTopics = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(t => new TopicListItemViewModel
        {
            Id = t.Id,
            Titulo = t.Titulo,
            UsuarioNome = t.Usuario != null ? t.Usuario.Nome : "Usuário Deletado",
            CategoriaId = t.CategoriaId,
            CategoriaNome = t.Categoria != null ? t.Categoria.Nome : "Sem Categoria",
            TotalRespostas = t.Respostas.Count(),
            TotalCurtidas = t.Curtidas.Count(),
            UsuarioCurtiu = userId.HasValue && t.Curtidas.Any(c => c.UsuarioId == userId.Value)
        }).ToListAsync();

    var result = new PagedResultViewModel<TopicListItemViewModel>
    {
        Items = pagedTopics,
        CurrentPage = page,
        TotalPages = totalPages,
        HasNextPage = page < totalPages
    };

    return Ok(result);
}

        // GET: api/topicos/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTopico(int id)
        {
            var userId = User.Identity.IsAuthenticated
                ? int.Parse(User.FindFirstValue("UserId"))
                : (int?)null;

            var topicoEntity = await _context.Topicos
                .Include(t => t.Usuario)
                .Include(t => t.Categoria)
                .Include(t => t.Curtidas)
                .Include(t => t.Anexos)
                .Include(t => t.Respostas).ThenInclude(r => r.Usuario)
                .Include(t => t.Respostas).ThenInclude(r => r.Curtidas)
                .Include(t => t.Respostas).ThenInclude(r => r.Anexos)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (topicoEntity == null) return NotFound();

            var topicoViewModel = new TopicDetailViewModel
            {
                Id = topicoEntity.Id,
                Titulo = topicoEntity.Titulo,
                Conteudo = topicoEntity.Conteudo,
                DataCriacao = topicoEntity.DataCriacao,
                EditadoEm = topicoEntity.EditadoEm,
                UsuarioId = topicoEntity.UsuarioId,
                UsuarioNome = topicoEntity.Usuario.Nome,
                CategoriaId = topicoEntity.CategoriaId,
                CategoriaNome = topicoEntity.Categoria.Nome,
                TotalCurtidas = topicoEntity.Curtidas.Count(),
                UsuarioCurtiu = userId.HasValue && topicoEntity.Curtidas.Any(c => c.UsuarioId == userId.Value),
                Anexos = topicoEntity.Anexos.Select(a => new AnexoViewModel { Id = a.Id, NomeArquivo = a.NomeArquivo, Url = a.Url, TipoConteudo = a.TipoConteudo, IsCarouselImage = a.IsCarouselImage }).ToList()
            };

            var todasAsRespostas = topicoEntity.Respostas.Select(r => new RespostaViewModel
            {
                Id = r.Id,
                UsuarioId = r.UsuarioId,
                UsuarioNome = r.Excluida ? "Usuário" : r.Usuario.Nome,
                Conteudo = r.Excluida ? "[Comentário removido]" : r.Conteudo,
                DataCriacao = r.DataCriacao,
                EditadoEm = r.EditadoEm,
                TotalCurtidas = r.Curtidas.Count(),
                UsuarioCurtiu = userId.HasValue && r.Curtidas.Any(c => c.UsuarioId == userId.Value),
                RespostaPaiId = r.RespostaPaiId,
                Excluida = r.Excluida,
                Anexos = r.Anexos.Select(a => new AnexoViewModel { Id = a.Id, NomeArquivo = a.NomeArquivo, Url = a.Url, TipoConteudo = a.TipoConteudo, IsCarouselImage = a.IsCarouselImage }).ToList()
            }).ToList();
            var DicionarioRespostas = todasAsRespostas.ToDictionary(r => r.Id);
            foreach (var resposta in todasAsRespostas) { if (resposta.RespostaPaiId.HasValue && DicionarioRespostas.ContainsKey(resposta.RespostaPaiId.Value)) { DicionarioRespostas[resposta.RespostaPaiId.Value].RespostasFilhas.Add(resposta); } else { topicoViewModel.Respostas.Add(resposta); } }
            topicoViewModel.Respostas = topicoViewModel.Respostas.OrderBy(r => r.DataCriacao).ToList();
            return Ok(topicoViewModel);
        }

        // POST: api/topicos
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateTopico([FromBody] CreateTopicViewModel topicViewModel)
        {
            if (topicViewModel.Imagens?.Count > 6) return BadRequest("Não é permitido enviar mais de 6 imagens para o carrossel.");
            if (topicViewModel.OutrosAnexos?.Count > 2) return BadRequest("Não é permitido enviar mais de 2 outros anexos.");

            var userId = int.Parse(User.FindFirstValue("UserId"));
            var novoTopico = new Topico { Titulo = topicViewModel.Titulo, Conteudo = topicViewModel.Conteudo, DataCriacao = DateTime.UtcNow, CategoriaId = topicViewModel.CategoriaId, UsuarioId = userId };
            
            if (topicViewModel.Imagens != null) { foreach (var anexo in topicViewModel.Imagens) { novoTopico.Anexos.Add(new Anexo { Url = anexo.Url, NomeArquivo = anexo.NomeArquivo, TipoConteudo = anexo.TipoConteudo, IsCarouselImage = true }); } }
            if (topicViewModel.OutrosAnexos != null) { foreach (var anexo in topicViewModel.OutrosAnexos) { novoTopico.Anexos.Add(new Anexo { Url = anexo.Url, NomeArquivo = anexo.NomeArquivo, TipoConteudo = anexo.TipoConteudo, IsCarouselImage = false }); } }
            
            _context.Topicos.Add(novoTopico);
            await _context.SaveChangesAsync();
            return Ok(new { id = novoTopico.Id });
        }

        // DELETE: api/topicos/x
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteTopico(int id)
        {
            var userId = int.Parse(User.FindFirstValue("UserId"));
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var topico = await _context.Topicos.FindAsync(id);
            if (topico == null) return NotFound();
            if (topico.UsuarioId != userId && userRole != "3") return Forbid(); 
            _context.Topicos.Remove(topico);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PUT: api/topicos/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateTopico(int id, [FromForm] UpdateTopicViewModel topicViewModel)
        {
            if (topicViewModel.Imagens?.Count > 6) return BadRequest("Não é permitido adicionar mais de 6 imagens.");
            if (topicViewModel.OutrosAnexos?.Count > 2) return BadRequest("Não é permitido adicionar mais de 2 outros anexos.");

            var userId = int.Parse(User.FindFirstValue("UserId"));
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var topico = await _context.Topicos.Include(t => t.Anexos).FirstOrDefaultAsync(t => t.Id == id);
            
            if (topico == null) return NotFound();
            if (topico.UsuarioId != userId && userRole != "3") return Forbid(); 
            
            topico.Titulo = topicViewModel.Titulo;
            topico.Conteudo = topicViewModel.Conteudo;
            topico.EditadoEm = DateTime.UtcNow;

            if (topicViewModel.Imagens != null)
            {
                var anexosAntigos = topico.Anexos.Where(a => a.IsCarouselImage).ToList();
                foreach (var anexo in anexosAntigos) { await _fileStorageService.DeleteFileAsync(anexo.Url); _context.Anexos.Remove(anexo); }
                foreach (var file in topicViewModel.Imagens) { var anexoUrl = await _fileStorageService.SaveFileAsync(file); topico.Anexos.Add(new Anexo { NomeArquivo = file.FileName, Url = anexoUrl, TipoConteudo = file.ContentType, IsCarouselImage = true }); }
            }
            if (topicViewModel.OutrosAnexos != null)
            {
                var outrosAnexosAntigos = topico.Anexos.Where(a => !a.IsCarouselImage).ToList();
                foreach (var anexo in outrosAnexosAntigos) { await _fileStorageService.DeleteFileAsync(anexo.Url); _context.Anexos.Remove(anexo); }
                foreach (var file in topicViewModel.OutrosAnexos) { var anexoUrl = await _fileStorageService.SaveFileAsync(file); topico.Anexos.Add(new Anexo { NomeArquivo = file.FileName, Url = anexoUrl, TipoConteudo = file.ContentType, IsCarouselImage = false }); }
            }
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/topicos/{id}/curtir
        [HttpPost("{id}/curtir")]
        [Authorize]
        public async Task<IActionResult> CurtirTopico(int id)
        {
            var userId = int.Parse(User.FindFirstValue("UserId"));
            var usuarioQueCurtiu = await _context.Usuarios.FindAsync(userId);
            var topico = await _context.Topicos.FindAsync(id);
            if (topico == null) return NotFound("Tópico não encontrado.");
            var curtidaExistente = await _context.Curtidas.AnyAsync(c => c.TopicoId == id && c.UsuarioId == userId);
            if (curtidaExistente) return BadRequest("Você já curtiu este tópico.");
            _context.Curtidas.Add(new Curtida { UsuarioId = userId, TopicoId = id, Data = DateTime.UtcNow });
            if (topico.UsuarioId != userId)
            {
                var mensagem = $"{usuarioQueCurtiu.Nome} curtiu seu tópico '{topico.Titulo}'.";
                var notificacaoExistente = await _context.Notificacoes.AnyAsync(n => n.UsuarioId == topico.UsuarioId && n.LinkId == topico.Id && n.Mensagem == mensagem && !n.Lida);
                if (!notificacaoExistente) { _context.Notificacoes.Add(new Notificacao { UsuarioId = topico.UsuarioId, Mensagem = mensagem, LinkId = topico.Id }); }
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        // DELETE: api/topicos/{id}/curtir
        [HttpDelete("{id}/curtir")]
        [Authorize]
        public async Task<IActionResult> DescurtirTopico(int id)
        {
            var userId = int.Parse(User.FindFirstValue("UserId"));
            var curtida = await _context.Curtidas.FirstOrDefaultAsync(c => c.TopicoId == id && c.UsuarioId == userId);
            if (curtida == null) return BadRequest("Você não curtiu este tópico para poder descurtir.");
            _context.Curtidas.Remove(curtida);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // GET: api/topicos/search?q=texto
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchTopicos([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Ok(new List<TopicListItemViewModel>());
            }

            var userId = User.Identity.IsAuthenticated
                ? int.Parse(User.FindFirstValue("UserId"))
                : (int?)null;

            var queryLower = q.ToLower();

            var topicosEncontrados = await _context.Topicos
                .Where(t => !t.Excluido)
                .Include(t => t.Usuario)
                .Include(t => t.Categoria)
                .Include(t => t.Respostas)
                .Include(t => t.Curtidas)
                .Where(t => t.Titulo.ToLower().Contains(queryLower) || t.Conteudo.ToLower().Contains(queryLower))
                .OrderByDescending(t => t.DataCriacao)
                .Select(t => new TopicListItemViewModel
                {
                    Id = t.Id,
                    Titulo = t.Titulo,
                    UsuarioNome = t.Usuario != null ? t.Usuario.Nome : "Usuário Deletado",
                    CategoriaId = t.CategoriaId,
                    CategoriaNome = t.Categoria != null ? t.Categoria.Nome : "Sem Categoria",
                    TotalRespostas = t.Respostas.Count(),
                    TotalCurtidas = t.Curtidas.Count(),
                    UsuarioCurtiu = userId.HasValue && t.Curtidas.Any(c => c.UsuarioId == userId.Value)
                })
                .ToListAsync();

            return Ok(topicosEncontrados);
        }
    }
}