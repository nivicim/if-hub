namespace if_hub.ViewModels
{
    public class TopicDetailViewModel
    {
        public int Id { get; set; }
        public string? Titulo { get; set; }
        public string? Conteudo { get; set; }
        public DateTime DataCriacao { get; set; }
        public int UsuarioId { get; set; }
        public string? UsuarioNome { get; set; }
        public string? CategoriaNome { get; set; }
        public List<RespostaViewModel> Respostas { get; set; } = new();
    }
}
