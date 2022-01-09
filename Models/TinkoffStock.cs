using System;

namespace Db_Trigger.Models
{
    public class TinkoffStock
    {
        public Guid Id { get; set; }
        public string Figi { get; set; }
        public string Ticker { get; set; }
        public string Currency { get; set; }
        public string Name { get; set; }
        public string CountryOfRisk { get; set; } = "";
        public string CountryOfRiskName { get; set; }
        public string Sector { get; set; }

        public override string ToString()
        {
            return $"{Figi} {Ticker} {Currency} {Name} {CountryOfRisk} countrName = {CountryOfRiskName} {Sector}";
        }
    }
}