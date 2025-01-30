using AutoTraderApp.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Domain.Entities
{
    public class BrokerLog : BaseEntity
    {
        public Guid BrokerAccountId { get; set; }
        public string Symbol { get; set; }
        public string? Action { get; set; }
        public decimal? Price { get; set; }
        public int? Quantity { get; set; }
        public string Message { get; set; }
    }
}