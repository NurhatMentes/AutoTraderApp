using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Core.Constants
{
    public static class ApplicationConstants
    {
        public static class Authentication
        {
            public const int PasswordMinLength = 6;
            public const int PasswordMaxLength = 16;
            public const int AccessTokenExpiration = 10; // minutes
            public const int RefreshTokenExpiration = 60; // minutes
        }

        public static class Cache
        {
            public const int DefaultCacheTime = 60; // minutes
        }
    }
}
