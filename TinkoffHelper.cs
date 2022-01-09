using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Db_Trigger.Models.Response;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Db_Trigger
{
    public class TinkoffHelper
    {
        private readonly ILogger _logger;

        private JsonSerializerSettings jsonSettings = new()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            Formatting = Formatting.Indented
        };

        private HttpClient _client = new();
        private StockRepository _stockRepository;

        public TinkoffHelper(ILogger logger)
        {
            _logger = logger;
            _logger.LogInformation("Created Tinkoff Helper");
            var token = Environment.GetEnvironmentVariable("Token_Tinkoff");
            _logger.LogInformation($"token = {token}");

            _client.DefaultRequestHeaders.Add("accept", "application/json");
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            _logger.LogInformation("Create http client");

            _stockRepository = new StockRepository(logger);
        }

        public async Task<List<string>> AddStocks()
        {
            _logger.LogInformation("Start getting stocks from tinkoff");
            var body = JsonContent.Create(new
            {
                instrumentStatus = "INSTRUMENT_STATUS_ALL"
            });
            _logger.LogInformation($"Create body {body}");
            var response =
                await _client.PostAsync(
                    "https://invest-public-api.tinkoff.ru/rest/tinkoff.public.invest.api.contract.v1.InstrumentsService/Shares",
                    body
                );
            _logger.LogInformation($"Response http code {response.StatusCode}");
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TinkoffStocksResponse>(responseBody, jsonSettings);
            _logger.LogInformation($"Found {result.Instruments.Count} stocks");

            //await _stockRepository.AddOrUpdateStocks(result.Instruments);
            return result.Instruments.Select(instument => instument.Figi).ToList();
        }
        public async Task UpdatePrices(List<string> figis)
        {
            _logger.LogInformation("Start getting price of stocks from tinkoff");
            var body = JsonContent.Create(new
            {
                figi = figis
            });
            _logger.LogInformation($"Create body {body}");
            var response =
                await _client.PostAsync(
                    "https://invest-public-api.tinkoff.ru/rest/tinkoff.public.invest.api.contract.v1.MarketDataService/GetLastPrices",
                    body
                );
            _logger.LogInformation($"Response http code {response.StatusCode}");
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            var pricesResponse = JsonConvert.DeserializeObject<TinkoffLastPriceResponse>(responseBody, jsonSettings);
            _logger.LogInformation($"Found {pricesResponse.LastPrices.Count} prices of stocks");

            await _stockRepository.UpdateLastPrice(pricesResponse.LastPrices);
        }
    }
}