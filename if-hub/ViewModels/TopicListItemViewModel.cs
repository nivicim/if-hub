namespace if_hub.ViewModels
{
    public class TopicListItemViewModel
    {
        public int Id { get; set; }
        public string? Titulo { get; set; }
        public string? Conteudo { get; set; }
        public DateTime DataCriacao { get; set; }
        public string? UsuarioNome { get; set; }
        public string? CategoriaNome { get; set; }
        public int TotalRespostas { get; set; }
        public int TotalCurtidas { get; set; }
        public bool UsuarioCurtiu { get; set; }
    }
}
