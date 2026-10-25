namespace OrderManagement.Models
{
    public class TransportOrder
    {
        public string Id { get; set; } = string.Empty;
        public FromInfo From { get; set; } = new();
        public ToInfo To { get; set; } = new();
        public string Status { get; set; } = "CREATED";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
