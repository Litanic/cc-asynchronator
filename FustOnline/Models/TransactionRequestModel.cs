using Asynchronator.Interfaces;

namespace FustOnline.Models
{
    public class TransactionRequestModel : IBusMessage
    {
        public Guid CorrelationId { get; set; }
        public string? CustomerLocation { get; set; }
        public string? Location { get; set; }
        public int Status { get; set; }
        public int TransactionType { get; set; }
        public FustModel[]? Fusts { get; set; }
    }
}