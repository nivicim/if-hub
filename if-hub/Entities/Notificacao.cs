using System;
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

        // Link para o tópico onde a ação ocorreu
        public int LinkId { get; set; }

        // Usuário que vai receber a notificação
        [Required]
        public int UsuarioId { get; set; }
        public virtual Usuario Usuario { get; set; }
    }
}