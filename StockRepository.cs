using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Db_Trigger.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Db_Trigger
{
    public class StockRepository
    {
        private readonly ILogger _logger;
        private readonly string _dbConnectionString;

        public StockRepository(ILogger logger)
        {
            _logger = logger;
            _logger.LogInformation("Created stocks repository");
            _dbConnectionString = Environment.GetEnvironmentVariable("SqlConnectionString");

            _logger.LogInformation($"db connection string {_dbConnectionString}");
        }

        public async Task AddOrUpdateStocks(List<TinkoffStock> instruments)
        {
            var addedStocks = 0;
            await using var conn = new SqlConnection(_dbConnectionString);
            conn.Open();
            foreach (var stock in instruments)
            {
                var countRequest = "SELECT count(*) FROM Stocks where FIGI=@Figi";

                await using var countCommand = new SqlCommand(countRequest, conn);
                countCommand.Parameters.Add(new SqlParameter("Figi", stock.Figi));

                var rows = (int)await countCommand.ExecuteScalarAsync();
                if (rows != 0) continue;
                var insertRequest =
                    "INSERT INTO Stocks VALUES(@Id, @Figi, @Ticker, @Currency, @Name, @CountryOfRisk, @CountryOfRiskName, @Sector, @LastUpdate, 0)";
                await using var insertCommand = new SqlCommand(insertRequest, conn);
                _logger.LogInformation($"Try add {stock}");
                insertCommand.Parameters.Add(new SqlParameter("Id", Guid.NewGuid()));
                insertCommand.Parameters.Add(new SqlParameter("Figi", stock.Figi));
                insertCommand.Parameters.Add(new SqlParameter("Ticker", stock.Ticker));
                insertCommand.Parameters.Add(new SqlParameter("Currency", stock.Currency));
                insertCommand.Parameters.Add(new SqlParameter("Name", stock.Name));
                insertCommand.Parameters.Add(new SqlParameter("CountryOfRisk", stock.CountryOfRisk));
                if (string.IsNullOrEmpty(stock.CountryOfRiskName)) stock.CountryOfRiskName = stock.CountryOfRisk;
                insertCommand.Parameters.Add(new SqlParameter("CountryOfRiskName", stock.CountryOfRiskName));
                insertCommand.Parameters.Add(new SqlParameter("Sector", stock.Sector));
                insertCommand.Parameters.Add(new SqlParameter("LastUpdate", DateTime.Now));

                await insertCommand.ExecuteNonQueryAsync();
                _logger.LogInformation($"Successfuly added {stock}");
                addedStocks++;
            }

            await conn.CloseAsync();
            _logger.LogInformation($"Added new {addedStocks} stocks");
            _logger.LogInformation("Finished added stocks");
        }

        public async Task UpdateLastPrice(List<TinkoffInfoPrice> lastPrices)
        {
            await using var conn = new SqlConnection(_dbConnectionString);
            conn.Open();
            foreach (var lastPrice in lastPrices)
            {
                var countRequest = "SELECT count(*) FROM Stocks where FIGI=@Figi";
                await using var countCommand = new SqlCommand(countRequest, conn);
                countCommand.Parameters.Add(new SqlParameter("Figi", lastPrice.Figi));

                var rows = (int)await countCommand.ExecuteScalarAsync();
                if (rows == 0) continue;

                var updateRequest =
                    "UPDATE Stocks SET Price=@Price, LastUpdate=@LastUpdate WHERE FIGI=@Figi";
                await using var updateCommand = new SqlCommand(updateRequest, conn);
                if (!CheckEnitty(lastPrice)) continue;
                _logger.LogInformation($"Try update {lastPrice}");
                updateCommand.Parameters.Add(new SqlParameter("Figi", lastPrice.Figi));
                updateCommand.Parameters.Add(new SqlParameter("Price", int.Parse(lastPrice.Price.Units)));
                updateCommand.Parameters.Add(new SqlParameter("LastUpdate", lastPrice.Time));
                await updateCommand.ExecuteNonQueryAsync();
            }

            await conn.CloseAsync();
            _logger.LogInformation($"Successfuly updated prices");
        }

        private bool CheckEnitty(TinkoffInfoPrice lastPrice)
        {
            if (lastPrice == null || lastPrice.Price == null) return false;
            return !string.IsNullOrEmpty(lastPrice.Price.Units);
        }
    }
}