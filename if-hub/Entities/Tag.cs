namespace if_hub.Entities
{
    public class Tag
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public virtual ICollection<TopicoTag> TopicoTags { get; set; }
    }
}
