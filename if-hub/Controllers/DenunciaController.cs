using if_hub.Entities;
using if_hub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace if_hub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class DenunciasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DenunciasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/denuncias
        [HttpPost]
        public async Task<IActionResult> CreateDenuncia([FromBody] CreateDenunciaViewModel model)
        {
            if (!model.TopicoId.HasValue && !model.RespostaId.HasValue)
            {
                return BadRequest("É necessário especificar um tópico ou uma resposta para a denúncia.");
            }

            var autorId = int.Parse(User.FindFirstValue("UserId"));

            var novaDenuncia = new Denuncia
            {
                Motivo = model.Motivo,
                AutorId = autorId,
                TopicoId = model.TopicoId,
                RespostaId = model.RespostaId,
                Status = StatusDenuncia.Pendente
            };

            _context.Denuncias.Add(novaDenuncia);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}