using System.ComponentModel.DataAnnotations;

namespace if_hub.ViewModels
{
    public class AprovarDenunciaViewModel
    {
        [Required(ErrorMessage = "A justificativa é obrigatória.")]
        public string Justificativa { get; set; }
    }
}