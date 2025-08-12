namespace if_hub.Entities
{
    public class LogAcao
    {
        public int Id { get; set; }
        public string Acao { get; set; }
        public DateTime Data { get; set; }

        // Foreign Key
        public int UsuarioId { get; set; }

        // Navigation property
        public virtual Usuario Usuario { get; set; }
    }
}
