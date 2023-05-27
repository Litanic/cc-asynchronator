using FustOnline.Models;

namespace Asynchronator.Interfaces
{
    public interface IHttpCommandQueue : IObservable<HttpCommandModel>
    {
        HttpCommandModel AddToQueue<T>(T httpRequest) where T : IBusMessage;
        void RemoveFromQueue(Guid guid);
        void UpdateQueue(HttpCommandModel httpCommandModel);
        void UpdateQueue(Guid guid, object httpResponse);
    }
}
