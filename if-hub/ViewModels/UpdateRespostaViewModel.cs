namespace if_hub.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class UpdateRespostaViewModel
    {
        [Required]
        [StringLength(10000, MinimumLength = 1)]
        public string Conteudo { get; set; }
    }
}
