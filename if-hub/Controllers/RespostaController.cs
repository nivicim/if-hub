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
    [Authorize] 
    public class RespostasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RespostasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/respostas
        [HttpPost]
        public async Task<IActionResult> CreateResposta(CreateRespostaViewModel respostaViewModel)
        {
            // Obter o ID do usuário logado a partir do token
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized();
            }
            var userId = int.Parse(userIdString);

            // Verificar se o tópico ao qual se está respondendo realmente existe
            var topicoExiste = await _context.Topicos.AnyAsync(t => t.Id == respostaViewModel.TopicoId);
            if (!topicoExiste)
            {
                return BadRequest("O tópico especificado não existe.");
            }

            // Criar a nova entidade Resposta
            var novaResposta = new Resposta
            {
                Conteudo = respostaViewModel.Conteudo,
                DataCriacao = DateTime.UtcNow,
                TopicoId = respostaViewModel.TopicoId,
                UsuarioId = userId,
                RespostaPaiId = null
            };

            _context.Respostas.Add(novaResposta);
            await _context.SaveChangesAsync();

            return StatusCode(201, novaResposta);
        }

        // DELETE: api/respostas/x
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteResposta(int id)
        {
            // Obter o ID do usuário logado
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized();
            }
            var userId = int.Parse(userIdString);

            // Encontrar a resposta no banco
            var resposta = await _context.Respostas.FindAsync(id);

            if (resposta == null)
            {
                return NotFound();
            }

            if (resposta.UsuarioId != userId && userRole != "2" && userRole != "3")
            {
                return Forbid();
            }

            _context.Respostas.Remove(resposta);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/respostas/x
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateResposta(int id, UpdateRespostaViewModel respostaViewModel)
        {
            // Obter o ID do usuário logado
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized();
            }
            var userId = int.Parse(userIdString);

            // Encontrar a resposta no banco
            var resposta = await _context.Respostas.FindAsync(id);

            if (resposta == null)
            {
                return NotFound();
            }

            if (resposta.UsuarioId != userId && userRole != "2" && userRole != "3")
            {
                return Forbid();
            }

            // Se a permissão for válida, atualiza os dados
            resposta.Conteudo = respostaViewModel.Conteudo;
            resposta.EditadoEm = DateTime.UtcNow; 

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/respostas/{id}/curtir
        [HttpPost("{id}/curtir")]
        [Authorize]
        public async Task<IActionResult> CurtirResposta(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var resposta = await _context.Respostas.FindAsync(id);
            if (resposta == null)
            {
                return NotFound("Resposta não encontrada.");
            }

            var curtidaExistente = await _context.Curtidas
                .AnyAsync(c => c.RespostaId == id && c.UsuarioId == userId);

            if (curtidaExistente)
            {
                return BadRequest("Você já curtiu esta resposta.");
            }

            var novaCurtida = new Curtida
            {
                UsuarioId = userId,
                RespostaId = id,
                Data = DateTime.UtcNow
            };

            _context.Curtidas.Add(novaCurtida);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/respostas/{id}/curtir
        [HttpDelete("{id}/curtir")]
        [Authorize]
        public async Task<IActionResult> DescurtirResposta(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var curtida = await _context.Curtidas
                .FirstOrDefaultAsync(c => c.RespostaId == id && c.UsuarioId == userId);

            if (curtida == null)
            {
                return NotFound("Curtida não encontrada.");
            }

            _context.Curtidas.Remove(curtida);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
