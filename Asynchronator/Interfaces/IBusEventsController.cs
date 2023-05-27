using Asynchronator.Logic;
using FustOnline.Models;
using RabbitMQ.Client;

namespace Asynchronator.Interfaces
{
    public interface IBusEvents : IObserver<HttpCommandModel>
    {
        public void Listen(IModel channel, IBasicProperties props);
        public void Send(HttpCommandModel httpCommandModel);
        void Subscribe(HttpCommandQueue httpCommandQueue);
        void Unsubscribe();
    }
}