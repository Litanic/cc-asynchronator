using FustOnline.Logic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Asynchronator.Logic
{
    public abstract class AsynchronatorControllerBase : ControllerBase
    {
        public readonly HttpCommandProcessor _commandProcessor;
        public readonly ILogger<AsynchronatorControllerBase> _logger;

        public AsynchronatorControllerBase(ILogger<AsynchronatorControllerBase> logger, HttpCommandProcessor commandProcessor)
        {
            _logger = logger;
            _commandProcessor = commandProcessor;
        }
    }
}
