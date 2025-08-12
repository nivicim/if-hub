namespace if_hub.Entities
{
    public class Topico
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Conteudo { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? EditadoEm { get; set; }
        public int UsuarioId { get; set; }
        public int CategoriaId { get; set; }
        public virtual Usuario Usuario { get; set; }
        public virtual Categoria Categoria { get; set; }
        public virtual ICollection<Resposta> Respostas { get; set; }
        public virtual ICollection<Curtida> Curtidas { get; set; }
        public virtual ICollection<TopicoTag> TopicoTags { get; set; }
    }
}
