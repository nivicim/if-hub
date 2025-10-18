using if_hub.Entities;

namespace if_hub.Controllers
{
    using if_hub.ViewModels;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Security.Claims;

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "2,3")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/admin/users
        [HttpGet("users")]
        [Authorize(Roles = "3")] 
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Usuarios
                .Include(u => u.Role) 
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    Email = u.Email,
                    RoleNome = u.Role.Nome,
                    Banido = u.Banido,
                    PostsRemovidosCount = _context.LogsModeracao.Count(log => 
                        log.UsuarioAlvoId == u.Id && 
                        (log.Acao == TipoAcaoModeracao.RemocaoTopico || log.Acao == TipoAcaoModeracao.RemocaoResposta))
                })
                .ToListAsync();

            return Ok(users);
        }

        // PUT: api/admin/users/{userId}/role
        [HttpPut("users/{userId}/role")]
        [Authorize(Roles = "3")] // Apenas Admins podem mudar papéis
        public async Task<IActionResult> UpdateUserRole(int userId, [FromBody] UpdateUserRoleViewModel model)
        {
            var userToUpdate = await _context.Usuarios.FindAsync(userId);
            if (userToUpdate == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            var roleExists = await _context.Roles.AnyAsync(r => r.Id == model.NewRoleId);
            if (!roleExists)
            {
                return BadRequest("O papel (Role) especificado não existe.");
            }

            var loggedInAdminId = User.FindFirstValue("UserId");
            if (userToUpdate.Id.ToString() == loggedInAdminId)
            {
                return BadRequest("Administradores não podem alterar o próprio papel.");
            }

            userToUpdate.RoleId = model.NewRoleId;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("denuncias")]
        public async Task<IActionResult> GetDenunciasPendentes()
        {
            var denuncias = await _context.Denuncias
                .Include(d => d.Autor)
                .Include(d => d.Topico)
                .Include(d => d.Resposta) // O Include na Resposta é crucial
                .Where(d => d.Status == Entities.StatusDenuncia.Pendente)
                .OrderBy(d => d.DataCriacao)
                .Select(d => new DenunciaViewModel
                {
                    Id = d.Id,
                    Motivo = d.Motivo,
                    Status = d.Status.ToString(),
                    DataCriacao = d.DataCriacao,
                    AutorNome = d.Autor.Nome,
                    
                    TopicoId = d.TopicoId ?? d.Resposta.TopicoId, 
            
                    RespostaId = d.RespostaId,
                    ConteudoDenunciadoPreview = d.TopicoId.HasValue
                        ? d.Topico.Conteudo.Substring(0, Math.Min(d.Topico.Conteudo.Length, 100))
                        : d.Resposta.Conteudo.Substring(0, Math.Min(d.Resposta.Conteudo.Length, 100))
                })
                .ToListAsync();

            return Ok(denuncias);
        }

        // POST: api/admin/denuncias/{id}/rejeitar
        [HttpPost("denuncias/{id}/rejeitar")]
        public async Task<IActionResult> RejeitarDenuncia(int id)
        {
            var denuncia = await _context.Denuncias.FindAsync(id);
            if (denuncia == null || denuncia.Status != StatusDenuncia.Pendente)
            {
                return NotFound("Denúncia não encontrada ou já processada.");
            }

            denuncia.Status = StatusDenuncia.Rejeitada;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/admin/denuncias/{id}/aprovar
        [HttpPost("denuncias/{id}/aprovar")]
        public async Task<IActionResult> AprovarDenuncia(int id, [FromBody] AprovarDenunciaViewModel model)
        {
            var moderadorId = int.Parse(User.FindFirstValue("UserId"));

            var denuncia = await _context.Denuncias
                .Include(d => d.Topico)
                .Include(d => d.Resposta)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (denuncia == null || denuncia.Status != StatusDenuncia.Pendente)
            {
                return NotFound("Denúncia não encontrada ou já processada.");
            }

            denuncia.Status = StatusDenuncia.Aprovada;
    
            int usuarioAlvoId;
            TipoAcaoModeracao acao;

            if (denuncia.TopicoId.HasValue && denuncia.Topico != null)
            {
                usuarioAlvoId = denuncia.Topico.UsuarioId;
                acao = TipoAcaoModeracao.RemocaoTopico;
        
                denuncia.Topico.Excluido = true;
                denuncia.Topico.Conteudo = "[Tópico removido pela moderação]";
            }
            else if (denuncia.RespostaId.HasValue && denuncia.Resposta != null)
            {
                usuarioAlvoId = denuncia.Resposta.UsuarioId;
                acao = TipoAcaoModeracao.RemocaoResposta;
                denuncia.Resposta.Excluida = true;
                denuncia.Resposta.Conteudo = "[Conteúdo removido pela moderação]";
            }
            else
            {
                return BadRequest("O conteúdo denunciado não foi encontrado.");
            }

            var log = new LogModeracao
            {
                Acao = acao,
                Justificativa = model.Justificativa,
                ModeradorId = moderadorId,
                UsuarioAlvoId = usuarioAlvoId,
                DenunciaId = denuncia.Id
            };
            _context.LogsModeracao.Add(log);

            await _context.SaveChangesAsync();
            return NoContent();
        }
        
        // POST: api/admin/users/{id}/toggle-ban
        [HttpPost("users/{id}/toggle-ban")]
        [Authorize(Roles = "3")] 
        public async Task<IActionResult> ToggleBanUsuario(int id, [FromBody] BanUsuarioViewModel model)
        {
            var adminId = int.Parse(User.FindFirstValue("UserId"));

            var usuarioAlvo = await _context.Usuarios.FindAsync(id);
            if (usuarioAlvo == null)
            {
                return NotFound("Usuário não encontrado.");
            }
    
            if (usuarioAlvo.Id == adminId)
            {
                return BadRequest("Você não pode banir a si mesmo.");
            }

            usuarioAlvo.Banido = !usuarioAlvo.Banido;

            var log = new LogModeracao
            {
                Acao = TipoAcaoModeracao.BanimentoUsuario,
                Justificativa = model.Justificativa,
                ModeradorId = adminId,
                UsuarioAlvoId = usuarioAlvo.Id
            };
            _context.LogsModeracao.Add(log);

            await _context.SaveChangesAsync();
            return Ok(new { isBanned = usuarioAlvo.Banido });
        }
    }
}