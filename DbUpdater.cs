using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Db_Trigger
{
    public class DbUpdater
    {
        [FunctionName("DbUpdater")]
        public async Task RunAsync(
            [TimerTrigger("0 0 * * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation("Start execute");

            var tinkoffHelper = new TinkoffHelper(log);
            var figis = await tinkoffHelper.AddStocks();
            await tinkoffHelper.UpdatePrices(figis);

            log.LogInformation("Finished execute");
        }
    }
}