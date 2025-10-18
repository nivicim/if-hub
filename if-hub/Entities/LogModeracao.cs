using System.ComponentModel.DataAnnotations;

namespace if_hub.Entities
{
    public enum TipoAcaoModeracao
    {
        RemocaoTopico,
        RemocaoResposta,
        BanimentoUsuario
    }

    public class LogModeracao
    {
        [Key]
        public int Id { get; set; }

        public TipoAcaoModeracao Acao { get; set; }
        
        [Required]
        public string Justificativa { get; set; }

        public DateTime DataAcao { get; set; } = DateTime.UtcNow;

        public int ModeradorId { get; set; }
        public virtual Usuario Moderador { get; set; }

        public int UsuarioAlvoId { get; set; }
        public virtual Usuario UsuarioAlvo { get; set; }

        public int? DenunciaId { get; set; }
        public virtual Denuncia Denuncia { get; set; }
    }
}