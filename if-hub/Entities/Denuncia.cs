using System.ComponentModel.DataAnnotations;

namespace if_hub.Entities
{
    public enum StatusDenuncia
    {
        Pendente,
        Aprovada, 
        Rejeitada 
    }

    public class Denuncia
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Motivo { get; set; }

        public StatusDenuncia Status { get; set; } = StatusDenuncia.Pendente;

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public int AutorId { get; set; }
        public virtual Usuario Autor { get; set; }

        public int? TopicoId { get; set; }
        public virtual Topico Topico { get; set; }

        public int? RespostaId { get; set; }
        public virtual Resposta Resposta { get; set; }
    }
}