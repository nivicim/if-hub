namespace if_hub.ViewModels
{
    public class NotificacaoViewModel
    {
        public int Id { get; set; }
        public string Mensagem { get; set; }
        public bool Lida { get; set; }
        public DateTime DataEnvio { get; set; }
        public int LinkId { get; set; } 
    }
}