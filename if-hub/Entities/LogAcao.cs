namespace if_hub.Entities
{
    public class LogAcao
    {
        public int Id { get; set; }
        public string Acao { get; set; }
        public DateTime Data { get; set; }

        public int UsuarioId { get; set; }

        public virtual Usuario Usuario { get; set; }
    }
}
