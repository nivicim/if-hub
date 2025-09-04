namespace if_hub.ViewModels
{
    public class RespostaViewModel
    {
        public int Id { get; set; }
        public string? Conteudo { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? EditadoEm { get; set; }
        public int UsuarioId { get; set; }
        public string? UsuarioNome { get; set; }
        public int TotalCurtidas { get; set; }
        public bool UsuarioCurtiu { get; set; }
        public int? RespostaPaiId { get; set; }
        public bool Excluida { get; set; }
        public List<RespostaViewModel> RespostasFilhas { get; set; } = new List<RespostaViewModel>();
        public List<AnexoViewModel> Anexos { get; set; } = new();
    }
}
