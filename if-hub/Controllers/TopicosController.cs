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
                CategoriaId = t.CategoriaId,
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

            var topicoEntity = await _context.Topicos
                .Include(t => t.Usuario)
                .Include(t => t.Categoria)
                .Include(t => t.Curtidas)
                .Include(t => t.Respostas).ThenInclude(r => r.Usuario)
                .Include(t => t.Respostas).ThenInclude(r => r.Curtidas)
                .Include(t => t.Anexos) 
                .Include(t => t.Respostas).ThenInclude(r => r.Anexos)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (topicoEntity == null)
            {
                return NotFound();
            }

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
                        Anexos = topicoEntity.Anexos.Select(a => new AnexoViewModel
                        {
                            Id = a.Id,
                            NomeArquivo = a.NomeArquivo,
                            Url = a.Url,
                            TipoConteudo = a.TipoConteudo,
                            IsCarouselImage = a.IsCarouselImage
                        }).ToList()
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
                Anexos = r.Anexos.Select(a => new AnexoViewModel
                {
                    Id = a.Id,
                    NomeArquivo = a.NomeArquivo,
                    Url = a.Url,
                    TipoConteudo = a.TipoConteudo,
                    IsCarouselImage = a.IsCarouselImage
                }).ToList()
            }).ToList();

            var DicionarioRespostas = todasAsRespostas.ToDictionary(r => r.Id);

            foreach (var resposta in todasAsRespostas)
            {
                if (resposta.RespostaPaiId.HasValue && DicionarioRespostas.ContainsKey(resposta.RespostaPaiId.Value))
                {
                    DicionarioRespostas[resposta.RespostaPaiId.Value].RespostasFilhas.Add(resposta);
                }
                else
                {
                    topicoViewModel.Respostas.Add(resposta);
                }
            }

            topicoViewModel.Respostas = topicoViewModel.Respostas.OrderBy(r => r.DataCriacao).ToList();

            return Ok(topicoViewModel);
        }

        // POST: api/topicos
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateTopico([FromBody] CreateTopicViewModel topicViewModel)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var novoTopico = new Topico
            {
                Titulo = topicViewModel.Titulo,
                Conteudo = topicViewModel.Conteudo,
                DataCriacao = DateTime.UtcNow,
                CategoriaId = topicViewModel.CategoriaId,
                UsuarioId = userId
            };

            // Processa a lista de Imagens do Carrossel
            if (topicViewModel.Imagens != null)
            {
                foreach (var anexo in topicViewModel.Imagens)
                {
                    novoTopico.Anexos.Add(new Anexo
                    {
                        Url = anexo.Url,
                        NomeArquivo = anexo.NomeArquivo,
                        TipoConteudo = anexo.TipoConteudo,
                        IsCarouselImage = true 
                    });
                }
            }

            // Processa a lista de Outros Anexos
            if (topicViewModel.OutrosAnexos != null)
            {
                foreach (var anexo in topicViewModel.OutrosAnexos)
                {
                    novoTopico.Anexos.Add(new Anexo
                    {
                        Url = anexo.Url,
                        NomeArquivo = anexo.NomeArquivo,
                        TipoConteudo = anexo.TipoConteudo,
                        IsCarouselImage = false 
                    });
                }
            }

            _context.Topicos.Add(novoTopico);
            await _context.SaveChangesAsync();

            return Ok(new { id = novoTopico.Id });
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
        public async Task<IActionResult> UpdateTopico(int id, [FromForm] UpdateTopicViewModel topicViewModel)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var topico = await _context.Topicos.Include(t => t.Anexos).FirstOrDefaultAsync(t => t.Id == id);

            if (topico == null)
            {
                return NotFound();
            }

            if (topico.UsuarioId != userId && userRole != "2" && userRole != "3")
            {
                return Forbid();
            }

            // Atualiza os campos de texto
            topico.Titulo = topicViewModel.Titulo;
            topico.Conteudo = topicViewModel.Conteudo;
            topico.EditadoEm = DateTime.UtcNow;

            // Processa a lista de novas Imagens
            if (topicViewModel.Imagens != null && topicViewModel.Imagens.Any())
            {
                foreach (var file in topicViewModel.Imagens)
                {
                    var anexoUrl = await _fileStorageService.SaveFileAsync(file);
                    topico.Anexos.Add(new Anexo
                    {
                        NomeArquivo = file.FileName,
                        Url = anexoUrl,
                        TipoConteudo = file.ContentType,
                        TamanhoEmBytes = file.Length
                    });
                }
            }

            // Processa a lista de Outros Anexos
            if (topicViewModel.OutrosAnexos != null && topicViewModel.OutrosAnexos.Any())
            {
                foreach (var file in topicViewModel.OutrosAnexos)
                {
                    var anexoUrl = await _fileStorageService.SaveFileAsync(file);
                    topico.Anexos.Add(new Anexo
                    {
                        NomeArquivo = file.FileName,
                        Url = anexoUrl,
                        TipoConteudo = file.ContentType,
                        TamanhoEmBytes = file.Length
                    });
                }
            }

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