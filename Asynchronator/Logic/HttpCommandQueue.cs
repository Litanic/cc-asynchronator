using Asynchronator.Interfaces;
using FustOnline.Models;

namespace Asynchronator.Logic
{
    /// <summary>
    /// Observable command queue.
    /// </summary>
    public sealed class HttpCommandQueue : IHttpCommandQueue
    {
        private readonly object _lock = new();

        public IList<IObserver<HttpCommandModel>> _observers;
        public IList<HttpCommandModel> _commandQueue;

        public HttpCommandQueue()
        {
            _observers = new List<IObserver<HttpCommandModel>>();
            _commandQueue = new List<HttpCommandModel>();
        }

        public IDisposable Subscribe(IObserver<HttpCommandModel> observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);

                foreach (var item in _commandQueue)
                    observer.OnNext(item);
            }

            return new Unsubscriber<HttpCommandModel>(_observers, observer);
        }

        public HttpCommandModel AddToQueue<T>(T httpRequest) where T : IBusMessage
        {
            httpRequest.CorrelationId = Guid.NewGuid(); // ToDo: Replace for msg bus correlationId.

            var httpCommand = new HttpCommandModel()
            {
                CorrelationId = Guid.NewGuid(), // Internal identifier.
                HttpRequest = httpRequest
            };

            lock (_lock)
                _commandQueue.Add(httpCommand);

            foreach (var observer in _observers)
                observer.OnNext(httpCommand);

            return httpCommand;
        }

        public void RemoveFromQueue(Guid guid)
        {
            lock (_lock)
            {
                var item = _commandQueue.FirstOrDefault(o => o.CorrelationId == guid);
                if (item == null) return; // Queued item was already removed -> no action needed.

                _commandQueue.Remove(item);
            }
        }

        public void UpdateQueue(HttpCommandModel httpCommand)
        {
            lock (_lock)
            {
                var item = _commandQueue.FirstOrDefault(o => o.CorrelationId == httpCommand.CorrelationId);
                if (item == null) return;

                item = httpCommand;

                foreach (var observer in _observers)
                    observer.OnNext(item);
            }
        }

        public void UpdateQueue(Guid correlationId, object httpResponse)
        {
            var item = _commandQueue.FirstOrDefault(o => o.HttpRequest?.CorrelationId == correlationId);
            if (item == null) return;

            item.HttpResponse = httpResponse;
            UpdateQueue(item);
        }
    }
}
