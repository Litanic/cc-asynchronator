using Asynchronator.Interfaces;

namespace FustOnline.Models
{
    public class HttpCommandModel
    {
        public Guid CorrelationId { get; set; }
        public IBusMessage? HttpRequest { get; set; }
        public object? HttpResponse { get; set; }
        public bool IsRequestSent { get; set; }
        public bool HasResponse { get; set; }
    }
}
