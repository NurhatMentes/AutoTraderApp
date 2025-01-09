using AutoTraderApp.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Domain.Entities
{
    public class CombinedStock : BaseEntity
    {
        public string Symbol { get; set; } 
        public string Category { get; set; } // Örn: "TopGainers", "TopLosers", "MostActive"
        public decimal? Price { get; set; } 
        public decimal? ChangePercentage { get; set; } 
        public long? Volume { get; set; }
    }
}
