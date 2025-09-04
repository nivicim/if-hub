using if_hub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace if_hub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnexosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorageService;

        public AnexosController(ApplicationDbContext context, IFileStorageService fileStorageService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
        }

        // DELETE: api/anexos/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAnexo(int id)
        {
            var anexo = await _context.Anexos.FindAsync(id);
            if (anexo == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            // Verifica se o usuário é o dono do conteúdo pai (tópico ou resposta)
            var ehDono = false;
            if (anexo.TopicoId.HasValue)
            {
                var topico = await _context.Topicos.FindAsync(anexo.TopicoId.Value);
                if (topico != null && topico.UsuarioId == userId)
                {
                    ehDono = true;
                }
            }
            else if (anexo.RespostaId.HasValue)
            {
                var resposta = await _context.Respostas.FindAsync(anexo.RespostaId.Value);
                if (resposta != null && resposta.UsuarioId == userId)
                {
                    ehDono = true;
                }
            }

            // Permite a exclusão se for o dono OU se for moderador/admin
            if (!ehDono && userRole != "2" && userRole != "3")
            {
                return Forbid();
            }

            // Deleta o arquivo físico e depois o registro no banco
            await _fileStorageService.DeleteFileAsync(anexo.Url);
            _context.Anexos.Remove(anexo);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}