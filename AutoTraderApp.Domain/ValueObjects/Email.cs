using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Domain.ValueObjects
{
    public class Email
    {
        public string Address { get; }

        public Email(string address)
        {
            if (string.IsNullOrWhiteSpace(address) || !address.Contains("@"))
                throw new ArgumentException("Geçersiz e-posta adresi.");

            Address = address;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Email email) return false;
            return Address.Equals(email.Address, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode() => Address.GetHashCode();
        public override string ToString() => Address;
    }
}
