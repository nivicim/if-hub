namespace if_hub.ViewModels;

public class DenunciaViewModel
{
    public int Id { get; set; }
    public string Motivo { get; set; }
    public string Status { get; set; }
    public DateTime DataCriacao { get; set; }
    public string AutorNome { get; set; }
    public int? TopicoId { get; set; }
    public int? RespostaId { get; set; }
    public string ConteudoDenunciadoPreview { get; set; }
}