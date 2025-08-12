using System.ComponentModel.DataAnnotations;

namespace if_hub.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string Nome { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Senha { get; set; }
    }
}
