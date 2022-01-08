using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Db_Trigger
{
    public class DbUpdater
    {
        [FunctionName("DbUpdater")]
        public async Task RunAsync([TimerTrigger("* */5 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            var client = new HttpClient();
            var response = await client.GetAsync("http://www.contoso.com/");
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            log.LogInformation($"Test body: {responseBody}");
            log.LogInformation("Ok");
        }
    }
}