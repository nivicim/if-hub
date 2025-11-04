namespace if_hub.Controllers
{
    using if_hub.Entities;
    using if_hub.ViewModels;
    using if_hub.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Security.Claims;

    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class RespostasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorageService;

        public RespostasController(ApplicationDbContext context, IFileStorageService fileStorageService)
        {
            _context = context;
            _fileStorageService = fileStorageService; 
        }

        // POST: api/respostas
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateResposta([FromForm] CreateRespostaViewModel respostaViewModel)
        {
            if (respostaViewModel.Anexos?.Count > 3) return BadRequest("Não é permitido enviar mais de 3 anexos por resposta.");
            var userId = int.Parse(User.FindFirstValue("UserId"));
            var usuarioQueRespondeu = await _context.Usuarios.FindAsync(userId);
            var novaResposta = new Resposta { Conteudo = respostaViewModel.Conteudo, DataCriacao = DateTime.UtcNow, TopicoId = respostaViewModel.TopicoId, UsuarioId = userId, RespostaPaiId = respostaViewModel.RespostaPaiId };
            if (respostaViewModel.Anexos != null) { foreach (var file in respostaViewModel.Anexos) { var anexoUrl = await _fileStorageService.SaveFileAsync(file); novaResposta.Anexos.Add(new Anexo { NomeArquivo = file.FileName, Url = anexoUrl, TipoConteudo = file.ContentType, TamanhoEmBytes = file.Length, IsCarouselImage = false }); } }
            _context.Respostas.Add(novaResposta);
            var topico = await _context.Topicos.FindAsync(respostaViewModel.TopicoId);
            if (topico.UsuarioId != userId) { var notificacaoTopico = new Notificacao { UsuarioId = topico.UsuarioId, Mensagem = $"{usuarioQueRespondeu.Nome} respondeu ao seu tópico '{topico.Titulo}'.", LinkId = topico.Id }; _context.Notificacoes.Add(notificacaoTopico); }
            if (respostaViewModel.RespostaPaiId.HasValue)
            {
                var respostaPai = await _context.Respostas.FindAsync(respostaViewModel.RespostaPaiId.Value);
                if (respostaPai.UsuarioId != userId && respostaPai.UsuarioId != topico.UsuarioId) { var notificacaoResposta = new Notificacao { UsuarioId = respostaPai.UsuarioId, Mensagem = $"{usuarioQueRespondeu.Nome} respondeu ao seu comentário.", LinkId = topico.Id }; _context.Notificacoes.Add(notificacaoResposta); }
            }
            await _context.SaveChangesAsync();
            return Ok();
        }


        // DELETE: api/respostas/x
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteResposta(int id)
        {
            var userId = int.Parse(User.FindFirstValue("UserId"));
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var resposta = await _context.Respostas.FindAsync(id);
            if (resposta == null) return NotFound();
            if (resposta.UsuarioId != userId && userRole != "3") return Forbid();
            resposta.Excluida = true;
            resposta.Conteudo = "[Comentário removido pelo autor ou moderador]";
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PUT: api/respostas/x
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateResposta(int id, [FromBody] UpdateRespostaViewModel model)
        {
            var userId = int.Parse(User.FindFirstValue("UserId"));
            var resposta = await _context.Respostas.FindAsync(id);
            if (resposta == null) return NotFound();
            if (resposta.UsuarioId != userId) return Forbid();
            resposta.Conteudo = model.Conteudo;
            resposta.EditadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/respostas/{id}/curtir
        [HttpPost("{id}/curtir")]
        [Authorize]
        public async Task<IActionResult> CurtirResposta(int id)
        {
            var userId = int.Parse(User.FindFirstValue("UserId"));
            var usuarioQueCurtiu = await _context.Usuarios.FindAsync(userId);
            var resposta = await _context.Respostas.Include(r => r.Topico).FirstOrDefaultAsync(r => r.Id == id);
            if (resposta == null) return NotFound("Resposta não encontrada.");
            var curtidaExistente = await _context.Curtidas.AnyAsync(c => c.RespostaId == id && c.UsuarioId == userId);
            if (curtidaExistente) return BadRequest("Você já curtiu esta resposta.");
            _context.Curtidas.Add(new Curtida { UsuarioId = userId, RespostaId = id, Data = DateTime.UtcNow });
            if (resposta.UsuarioId != userId)
            {
                var mensagem = $"{usuarioQueCurtiu.Nome} curtiu sua resposta no tópico '{resposta.Topico.Titulo}'.";
                var notificacaoExistente = await _context.Notificacoes.AnyAsync(n => n.UsuarioId == resposta.UsuarioId && n.LinkId == resposta.TopicoId && n.Mensagem == mensagem && !n.Lida);
                if (!notificacaoExistente) { _context.Notificacoes.Add(new Notificacao { UsuarioId = resposta.UsuarioId, Mensagem = mensagem, LinkId = resposta.TopicoId }); }
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        // DELETE: api/respostas/{id}/curtir
        [HttpDelete("{id}/curtir")]
        [Authorize]
        [HttpDelete("{id}/curtir")]
        [Authorize]
        public async Task<IActionResult> DescurtirResposta(int id)
        {
            var userId = int.Parse(User.FindFirstValue("UserId"));
            var curtida = await _context.Curtidas.FirstOrDefaultAsync(c => c.RespostaId == id && c.UsuarioId == userId);
            if (curtida == null) return BadRequest("Você não curtiu esta resposta para poder descurtir.");
            _context.Curtidas.Remove(curtida);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
