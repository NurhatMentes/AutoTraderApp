using AutoTraderApp.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Domain.Entities
{
    public class AlpacaApiLog : BaseEntity
    {
        public Guid BrokerAccountId { get; set; }
        public Guid UserId { get; set; }
        public string RequestUrl { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = string.Empty;
        public string? RequestBody { get; set; }
        public string? ResponseBody { get; set; }
        public int? ResponseStatusCode { get; set; }
        public string? ErrorMessage { get; set; }
        public Guid? TransactionId { get; set; }
        public string LogType { get; set; } = "Info"; 
    }
}