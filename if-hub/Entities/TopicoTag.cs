namespace if_hub.Entities
{
    public class TopicoTag
    {
        public int TopicoId { get; set; }
        public int TagId { get; set; }
        public virtual Topico Topico { get; set; }
        public virtual Tag Tag { get; set; }
    }
}
