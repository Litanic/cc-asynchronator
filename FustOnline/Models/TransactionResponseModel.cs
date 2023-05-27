using Asynchronator.Interfaces;

namespace FustOnline.Models
{
    public class TransactionResponseModel : IBusMessage
    {
        Guid IBusMessage.CorrelationId { get; set; }
        public string? CustomerLocation { get; set; }
        public string? CustomerNumber { get; set; }
        public string? SupplierNumber { get; set; }
        public string? Location { get; set; }
        public int Status { get; set; }
        public int TransactionType { get; set; }
    }
}