using Asynchronator.Interfaces;
using Asynchronator.Logic;
using FustOnline.Models;
using Microsoft.AspNetCore.Mvc;

namespace FustOnline.Logic
{
    public interface ICommandProcessor : IObserver<HttpCommandModel>
    {
        Task<ActionResult<TResponse>> ProcessSyncRequest<TRequest, TResponse>(TRequest httpRequest, int? simulatedArchitectureDelayMs) 
            where TRequest : IBusMessage
            where TResponse : IBusMessage;
        void Subscribe(HttpCommandQueue httpCommandQueue);
        void Unsubscribe();
    }
}
