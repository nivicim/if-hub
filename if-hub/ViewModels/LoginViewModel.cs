using System.ComponentModel.DataAnnotations;
namespace if_hub.ViewModels
{
    

    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Senha { get; set; }
    }
}
