using if_hub.Entities;
using if_hub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace if_hub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class NotificacoesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotificacoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/notificacoes
        [HttpGet]
        public async Task<IActionResult> GetNotificacoes()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var notificacoes = await _context.Notificacoes
                .Where(n => n.UsuarioId == userId)
                .OrderByDescending(n => n.DataEnvio)
                .Select(n => new NotificacaoViewModel
                {
                    Id = n.Id,
                    Mensagem = n.Mensagem,
                    Lida = n.Lida,
                    DataEnvio = n.DataEnvio,
                    LinkId = n.LinkId
                })
                .ToListAsync();

            return Ok(notificacoes);
        }

        // GET: api/notificacoes/nao-lidas/contagem
        [HttpGet("nao-lidas/contagem")]
        public async Task<IActionResult> GetContagemNaoLidas()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var contagem = await _context.Notificacoes
                .CountAsync(n => n.UsuarioId == userId && !n.Lida);

            return Ok(new { count = contagem });
        }

        // POST: api/notificacoes/marcar-todas-como-lidas
        [HttpPost("marcar-todas-como-lidas")]
        public async Task<IActionResult> MarcarTodasComoLidas()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var notificacoesNaoLidas = await _context.Notificacoes
                .Where(n => n.UsuarioId == userId && !n.Lida)
                .ToListAsync();

            if (notificacoesNaoLidas.Any())
            {
                foreach (var notificacao in notificacoesNaoLidas)
                {
                    notificacao.Lida = true;
                }
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }
    }
}