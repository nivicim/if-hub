using System.ComponentModel.DataAnnotations;

namespace if_hub.ViewModels
{
    public class CreateDenunciaViewModel
    {
        [Required(ErrorMessage = "O motivo da denúncia é obrigatório.")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "O motivo deve ter entre 10 e 500 caracteres.")]
        public string Motivo { get; set; }

        public int? TopicoId { get; set; }
        public int? RespostaId { get; set; }
    }
}