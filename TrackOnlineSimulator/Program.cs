namespace FustOnline
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var busEvents = new BusEventController(
                hostname: "localhost", 
                exchangeName: string.Empty, 
                upstreamQueue: "upstreamQueue",
                downstreamQueue: "downstreamQueue");
            busEvents.Run();
        }
    }
}
