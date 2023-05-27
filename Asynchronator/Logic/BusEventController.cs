using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using Asynchronator.Interfaces;
using FustOnline.Models;
using Asynchronator.Logic;
using Microsoft.Extensions.Logging;

namespace FustOnline.Logic
{
    public sealed class BusEventController : IBusEvents
    {
        private readonly ILogger<HttpCommandProcessor> _logger;
        private readonly string _hostname;
        private readonly string _exchangeName;
        private readonly string _downstreamQueue;
        private readonly string _upstreamQueue;
        private readonly HttpCommandQueue _httpCommandQueue;
        private readonly IModel _channel;
        private readonly IBasicProperties _props;
        private readonly IConnection _connection;
        private IDisposable? unsubscriber;

        public BusEventController(ILogger<HttpCommandProcessor> logger, HttpCommandQueue httpCommandQueue)
        {
            _logger = logger;

            // ToDo: Create appsettings for these:
            _hostname = "localhost";
            _exchangeName = string.Empty;
            _downstreamQueue = "downstreamQueue";
            _upstreamQueue = "upstreamQueue";

            _httpCommandQueue = httpCommandQueue;

            // Event bus setup.
            var factory = new ConnectionFactory { HostName = _hostname };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _downstreamQueue,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
            _channel.QueueDeclare(queue: _upstreamQueue,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            _props = _channel.CreateBasicProperties();
            _props.Persistent = true;

            Subscribe(_httpCommandQueue);
            _logger.Log(LogLevel.Debug, "Bus event controller started");
        }

        public void Listen(IModel channel, IBasicProperties props)
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var response = JsonSerializer.Deserialize<HttpCommandModel>(Encoding.UTF8.GetString(body));
                Console.WriteLine($"[x] Received {response}");
                _httpCommandQueue.UpdateQueue(response!.CorrelationId, response);
            };
            channel.BasicConsume(queue: _downstreamQueue,
                                 autoAck: true,
                                 consumer: consumer);
        }

        public void OnCompleted()
        {
            // ToDo: Proper completion handling.
        }

        public void OnError(Exception error)
        {
            // ToDo: Proper error handling.
        }

        public void OnNext(HttpCommandModel value)
        {
            Console.WriteLine(value);

            if (!value.IsRequestSent)
                Send(value);
        }

        public void Send(HttpCommandModel httpCommand)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(httpCommand.HttpRequest));

            _channel.BasicPublish(exchange: _exchangeName,
                                 routingKey: _upstreamQueue,
                                 basicProperties: _props,
                                 body: body);

            httpCommand.IsRequestSent = true;
            _httpCommandQueue.UpdateQueue(httpCommand);

            Console.WriteLine($"[x] Sent {httpCommand.HttpRequest}");
        }

        public void Subscribe(HttpCommandQueue httpCommandQueue)
        {
            unsubscriber = httpCommandQueue.Subscribe(this);
        }

        public void Unsubscribe()
        {
            unsubscriber?.Dispose();
        }

        public void Run()
        {
            Listen(_channel, _props);
        }
    }
}
