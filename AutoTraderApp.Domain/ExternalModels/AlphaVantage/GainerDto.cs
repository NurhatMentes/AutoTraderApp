using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Domain.ExternalModels.AlphaVantage
{
    public class GainerDto
    {
        public string Ticker { get; set; }
        public decimal Price { get; set; }
        public decimal ChangeAmount { get; set; }
        public string ChangePercentage { get; set; }
        public long Volume { get; set; }
    }
}
