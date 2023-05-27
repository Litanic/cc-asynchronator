using Asynchronator.Logic;
using FustOnline.Logic;
using FustOnline.Models;
using Microsoft.AspNetCore.Mvc;

namespace FustOnline.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FustOnlineController : AsynchronatorControllerBase
    {
        public FustOnlineController(ILogger<FustOnlineController> logger, HttpCommandProcessor commandProcessor) : base(logger, commandProcessor)
        {
        }

        /// <summary>
        /// Updates the transactions.
        /// </summary>
        /// <returns></returns>
        [HttpPost(Name = "transactions")]
        public async Task<ActionResult<TransactionResponseModel>> Transactions(CancellationToken cancellationToken, TransactionRequestModel transactions, int? simulatedControllerDelayMs, int? simulatedArchitectureDelayMs)
        {
            // !!!
            // ToDo: Remove me -> Creates artificial delay for this PoC.
            // This here is a weak point as the delay is not measured here.
            // The only solution is to never perform any logic here. Everything must be done by the command processor (which is also preferrable).
            // !!!!
            if (simulatedControllerDelayMs.HasValue) await Task.Delay(TimeSpan.FromMilliseconds(simulatedControllerDelayMs.Value));

            // This step should let the request be processed and awaited via the asynchronator.
            _logger.Log(LogLevel.Debug, "Performing ASR request");
            return await _commandProcessor.ProcessSyncRequest<TransactionRequestModel, TransactionResponseModel>(transactions, simulatedArchitectureDelayMs);
        }
    }
}
