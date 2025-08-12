namespace if_hub.Entities
{
    public class Curtida
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public int UsuarioId { get; set; }
        public int? TopicoId { get; set; }
        public int? RespostaId { get; set; }
        public virtual Usuario Usuario { get; set; }
        public virtual Topico Topico { get; set; }
        public virtual Resposta Resposta { get; set; }
    }
}
