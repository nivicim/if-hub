namespace if_hub.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class CreateRespostaViewModel
    {
        [Required]
        [StringLength(10000, MinimumLength = 1)]
        public string Conteudo { get; set; }

        [Required]
        public int TopicoId { get; set; }
    }
}
