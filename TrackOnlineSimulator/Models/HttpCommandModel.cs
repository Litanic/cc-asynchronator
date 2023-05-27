namespace FustOnline.Models
{
    public class HttpCommandModel
    {
        public Guid CorrelationId { get; set; }
        public TransactionModel? HttpResponse { get; set; }
    }
}
