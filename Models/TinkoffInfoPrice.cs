using System;

namespace Db_Trigger.Models
{
    public class TinkoffInfoPrice
    {
        public string Figi { get; set; }
        public DateTime Time { get; set; }
        public TinkoffPrice Price { get; set; }

        public override string ToString()
        {
            return $"Figi = {Figi} Time = {Time} Price = {Price.Units}";
        }
    }
}