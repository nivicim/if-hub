using if_hub.ViewModels; // Adicione este using
using System.ComponentModel.DataAnnotations;

public class CreateTopicViewModel
{
    [Required]
    [StringLength(100, MinimumLength = 5)]
    public string Titulo { get; set; }

    [Required]
    [StringLength(10000, MinimumLength = 10)]
    public string Conteudo { get; set; }

    [Required(ErrorMessage = "Por favor, selecione uma categoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "Por favor, selecione uma categoria válida.")]
    public int CategoriaId { get; set; }

    public List<AnexoViewModel>? Imagens { get; set; }

    public List<AnexoViewModel>? OutrosAnexos { get; set; }
}