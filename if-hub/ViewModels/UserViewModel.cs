namespace if_hub.ViewModels
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? RoleNome { get; set; }
        public bool Banido { get; set; }
        public int PostsRemovidosCount { get; set; }
    }
}
