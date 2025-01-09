using AutoTraderApp.Domain.ExternalModels.Polygon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Infrastructure.Interfaces
{
    public interface IPolygonService 
    {
        Task<decimal> GetStockPriceAsync(string symbol);
    }
}
