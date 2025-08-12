namespace if_hub.Entities
{
    public class Notificacao
    {
        public int Id { get; set; }
        public string Mensagem { get; set; }
        public bool Lida { get; set; } = false;
        public DateTime DataEnvio { get; set; }
        public int UsuarioId { get; set; }
        public virtual Usuario Usuario { get; set; }
    }
}
