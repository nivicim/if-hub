namespace if_hub.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string SenhaHash { get; set; }
        public DateTime DataCriacao { get; set; }
        public bool Ativo { get; set; } = true;
        public bool Banido { get; set; } = false;
        public int RoleId { get; set; }
        public virtual Role Role { get; set; }
        public virtual ICollection<Topico> Topicos { get; set; }
        public virtual ICollection<Resposta> Respostas { get; set; }
        public virtual ICollection<Curtida> Curtidas { get; set; } = new List<Curtida>();
        public virtual ICollection<Notificacao> Notificacoes { get; set; }
        public virtual ICollection<LogAcao> LogAcoes { get; set; }
    }
}
