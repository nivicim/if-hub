using System.ComponentModel.DataAnnotations;

namespace if_hub.Entities
{
    public class Anexo
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string NomeArquivo { get; set; }
        [Required]
        public string Url { get; set; }
        [Required]
        public string TipoConteudo { get; set; }
        public long TamanhoEmBytes { get; set; }
        public DateTime DataUpload { get; set; } = DateTime.UtcNow;
        public int? TopicoId { get; set; }
        public virtual Topico? Topico { get; set; }
        public int? RespostaId { get; set; }
        public virtual Resposta? Resposta { get; set; }
        public bool IsCarouselImage { get; set; } = false;
    }
}