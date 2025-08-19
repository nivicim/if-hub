namespace if_hub.Entities
{
    public class Resposta
    {
        public int Id { get; set; }
        public string Conteudo { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? EditadoEm { get; set; }
        public int UsuarioId { get; set; }
        public int TopicoId { get; set; }
        public int? RespostaPaiId { get; set; }
        public virtual Usuario Usuario { get; set; }
        public virtual Topico Topico { get; set; }
        public virtual Resposta RespostaPai { get; set; }
        public virtual ICollection<Resposta> RespostasFilhas { get; set; }
        public virtual ICollection<Curtida> Curtidas { get; set; } = new List<Curtida>();
    }
}
