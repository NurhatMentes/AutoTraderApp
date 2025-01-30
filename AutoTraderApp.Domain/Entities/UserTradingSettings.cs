using AutoTraderApp.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Domain.Entities
{
    public class UserTradingSetting : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public string BrokerType { get; set; }

        // Risk Yönetimi Parametreleri

        /// <summary>
        /// Kullanıcının her bir işlemde riske etmek istediği toplam hesap bakiyesinin yüzdesini temsil eder.
        /// Örneğin, %2 risk yüzdesi, 10.000 USD'lik bir hesap için 200 USD'lik bir risk anlamına gelir.
        /// </summary>
        public decimal RiskPercentage { get; set; } = 0.02m;

        /// <summary>
        /// Kullanıcının tek bir işlemde riske edebileceği maksimum tutarı belirtir.
        /// Risk yüzdesi hesaplamasından bağımsız olarak, tek bir işlemde riske edebilecekleri maksimum tutarı sınırlar.
        /// </summary>
        public decimal MaxRiskLimit { get; set; } = 2500;

        // Alım-Satım Parametreleri

        /// <summary>
        /// Kullanıcının bir hisse senedi için alım yaparken belirlediği minimum miktarı temsil eder.
        /// Küçük miktarlarda işlem yapmayı önlemek için kullanılır.
        /// </summary>
        public int MinBuyQuantity { get; set; } = 1;

        /// <summary>
        /// Kullanıcının bir hisse senedi için alım yaparken belirlediği maksimum miktarı temsil eder.
        /// Tek bir işlemde çok fazla miktarda alım yapmayı önlemek için kullanılır.
        /// </summary>
        public int MaxBuyQuantity { get; set; } = 100;

        /// <summary>
        /// Kullanıcının bir hisse senedini alırken mevcut fiyatın yüzdesi olarak belirlediği alım fiyatını temsil eder.
        /// Örneğin, %98 ayarlandığında, hisse senedi mevcut fiyatın %98'i üzerinden alınır.
        /// </summary>
        public decimal BuyPricePercentage { get; set; } = 0.98m;

        public decimal SellPricePercentage { get; set; } = 0.98m;
    }
}