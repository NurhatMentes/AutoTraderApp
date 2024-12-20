using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Application.Features.CombinedStocks.DTOs
{
    public class CombinedStockDto
    {
        public string Symbol { get; set; }
        public string Category { get; set; }
        public decimal? Price { get; set; }
        public decimal? ChangePercentage { get; set; }
        public long? Volume { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
