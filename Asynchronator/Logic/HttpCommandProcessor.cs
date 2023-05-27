using Asynchronator.Interfaces;
using Asynchronator.Logic;
using FustOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace FustOnline.Logic
{
    public sealed class HttpCommandProcessor : ICommandProcessor
    {
        private readonly ILogger<HttpCommandProcessor> _logger;
        private readonly HttpCommandQueue _httpCommandQueue;
        private IDisposable? _unsubscriber;
        private bool _hasResponse;
        private Guid _thisRequestGuid;

        // Create settings for these:
        private readonly TimeSpan _tcpTimeout = TimeSpan.FromSeconds(6); // TCP timeout is 300s as defined in RFC 793 and 240s for Windows machines.
        private readonly TimeSpan _observablePollRate = TimeSpan.FromMilliseconds(10); // Reduce the CPU load by increasing the poll rate.

        public HttpCommandProcessor(ILogger<HttpCommandProcessor> logger, HttpCommandQueue httpCommandQueue)
        {
            _logger = logger;
            _httpCommandQueue = httpCommandQueue;

            Subscribe(_httpCommandQueue);
            _logger.Log(LogLevel.Debug, "HTTP command processor started");
        }

        public void OnCompleted()
        {
            // ToDo: Completion handling.
        }

        public void OnError(Exception error)
        {
            // ToDo: Error handling.
        }

        public void OnNext(HttpCommandModel value)
        {
            if (value.CorrelationId == _thisRequestGuid && !value.HasResponse)
                _hasResponse = true;
        }

        public async Task<ActionResult<TResponse>> ProcessSyncRequest<TRequest, TResponse>(TRequest httpRequest, int? simulatedArchitectureDelayMs = null)
            where TRequest : IBusMessage
            where TResponse : IBusMessage
        {
            var queuedItem = _httpCommandQueue.AddToQueue(httpRequest);
            _thisRequestGuid = queuedItem.CorrelationId;

            var cancellationToken = new CancellationTokenSource(_tcpTimeout);

            // Validate stuff yo

            // ToDo: Remove me -> Creates artificial delay for this PoC.
            if (simulatedArchitectureDelayMs.HasValue) await Task.Delay(TimeSpan.FromMilliseconds(simulatedArchitectureDelayMs.Value));

            var hasResponse = false;
            HttpCommandModel? result = null;

            // Observe the command queue in a new async task and await its response.
            try
            {
                do
                {
                    if (_hasResponse == true)
                    {
                        result = _httpCommandQueue._commandQueue.FirstOrDefault(o => o.CorrelationId == queuedItem.CorrelationId);
                        if (result == null) return default!;
                        hasResponse = true;
                    }

                    // Reduce the CPU usage by introducing a polling rate.
                    await Task.Delay(_observablePollRate).WaitAsync(cancellationToken.Token);

                } while (!cancellationToken.IsCancellationRequested && !hasResponse);
            }
            catch (Exception ex) when (ex.Message == "A task was canceled.")
            {
                return new ObjectResult(HttpStatusCode.ServiceUnavailable);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e, e.Message, httpRequest);
            }

            if (result == null || result.HttpResponse == null) return default!;
            var httpCommandResponse = (HttpCommandModel)result.HttpResponse;
            return new OkObjectResult(httpCommandResponse.HttpResponse);
        }

        public void Subscribe(HttpCommandQueue httpCommandQueue)
        {
            _unsubscriber = httpCommandQueue.Subscribe(this);
        }

        public void Unsubscribe()
        {
            _unsubscriber?.Dispose();
        }
    }
}
