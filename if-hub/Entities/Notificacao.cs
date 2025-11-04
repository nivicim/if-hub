using System.ComponentModel.DataAnnotations;

namespace if_hub.Entities
{
    public class Notificacao
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Mensagem { get; set; }

        public bool Lida { get; set; } = false;

        public DateTime DataEnvio { get; set; } = DateTime.UtcNow;

        public int LinkId { get; set; }

        [Required]
        public int UsuarioId { get; set; }
        public virtual Usuario Usuario { get; set; }
    }
}