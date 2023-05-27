using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using FustOnline.Models;

namespace FustOnline
{
    public class BusEventController
    {
        private readonly string _hostname;
        private readonly string _exchangeName;
        private readonly string _upstreamQueue;
        private readonly string _downstreamQueue;
        private readonly IModel _channel;
        private readonly IBasicProperties _props;
        private readonly IConnection _connection;

        // Create settings for these:
        private readonly TimeSpan _tcpTimeout = TimeSpan.FromSeconds(180); // TCP timeout is 300s as defined in RFC 793 and 240s for Windows machines.

        public BusEventController(string hostname, string exchangeName, string upstreamQueue, string downstreamQueue)
        {
            _hostname = hostname;
            _exchangeName = exchangeName;
            _upstreamQueue = upstreamQueue;
            _downstreamQueue = downstreamQueue;

            // Event bus setup.
            var factory = new ConnectionFactory { HostName = _hostname };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _upstreamQueue,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
            _channel.QueueDeclare(queue: _downstreamQueue,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            _props = _channel.CreateBasicProperties();
            _props.Persistent = true;
        }

        public void Run()
        {
            // Start.
            Listen(_channel);
            Console.WriteLine($"[*] Waiting for messages on queue [{_upstreamQueue}]");
            do
            {
                // Nothing.
            } while (true);
        }

        public void Send(HttpCommandModel message)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            _channel.BasicPublish(exchange: _exchangeName,
                                 routingKey: _downstreamQueue,
                                 basicProperties: _props,
                                 body: body);
            Console.WriteLine($"[x] Sent {message} with correlationId {message.HttpResponse?.CorrelationId}");
        }

        private void Listen(IModel channel)
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var cancellationToken = new CancellationTokenSource(_tcpTimeout);
                var body = ea.Body.ToArray();
                var response = JsonSerializer.Deserialize<TransactionModel>(Encoding.UTF8.GetString(body))!;
                Console.WriteLine($"[x] Received response with correlationId {response.CorrelationId}");

                // PoC test: Simulate a processed message.
                var delay = new Random().Next(1000, 5000);
                await Task.Delay(delay).WaitAsync(cancellationToken.Token);

                SendResponse(response.CorrelationId);
                Console.WriteLine($"[x] [Simulated response] Response sent after {delay}ms");
            };
            channel.BasicConsume(queue: _upstreamQueue,
                                 autoAck: true,
                                 consumer: consumer);
        }

        private void SendResponse(Guid correlationId)
        {
            var httpCommand = new HttpCommandModel()
            {
                CorrelationId = correlationId,
                HttpResponse = new TransactionModel()
                {
                    CorrelationId = correlationId,
                    CustomerLocation = "Delft",
                    CustomerNumber = "customerNumber123",
                    Location = "Aalsmeer",
                    Status = 2,
                    SupplierNumber = "supplierNumber123",
                    TransactionType = 3
                }
            };
            Send(httpCommand);
        }
    }
}
