namespace Asynchronator.Interfaces
{
    public interface IBusMessage
    {
        public Guid CorrelationId { get; set; }
    }
}
