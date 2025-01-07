using AutoTraderApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Domain.ExternalModels.Alpaca.Models
{
    public class TradeResponse
    {
        public Trade Trade { get; set; }
        public string Symbol { get; set; }
    }
}
