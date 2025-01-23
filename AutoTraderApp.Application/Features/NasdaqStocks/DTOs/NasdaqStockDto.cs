using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Application.Features.NasdaqStocks.DTOs
{
    public class NasdaqStockDto
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public string Exchange { get; set; }
        public string AssetType { get; set; }
        public string IpoDate { get; set; }
        public string DelistingDate { get; set; }
        public string Status { get; set; }
    }
}
