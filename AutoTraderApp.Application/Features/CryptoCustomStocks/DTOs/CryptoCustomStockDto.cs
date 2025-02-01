using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Application.Features.CryptoCustomStocks.DTOs
{
    public class CryptoCustomStockDto
    {
        public Guid Id { get; set; }
        public string Symbol { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public Guid? UpdatedByUserId { get; set; }
    }
}
