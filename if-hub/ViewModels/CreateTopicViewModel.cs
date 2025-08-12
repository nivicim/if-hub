namespace if_hub.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class CreateTopicViewModel
    {
        [Required]
        [StringLength(100, MinimumLength = 5)]
        public string Titulo { get; set; }

        [Required]
        [StringLength(10000, MinimumLength = 10)]
        public string Conteudo { get; set; }

        [Required]
        public int CategoriaId { get; set; }
    }
}
