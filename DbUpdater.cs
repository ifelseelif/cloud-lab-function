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
        [FunctionName("Function1")]
        public async Task RunAsync([TimerTrigger("*/30 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync("http://www.contoso.com/");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            log.LogInformation($"Test body: {responseBody}");
        }
    }
}
