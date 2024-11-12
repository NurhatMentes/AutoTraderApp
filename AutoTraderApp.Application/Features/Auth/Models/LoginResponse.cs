using AutoTraderApp.Core.Security.JWT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Application.Features.Auth.Models
{
    public class LoginResponse
    {
        public AccessToken AccessToken { get; set; }
        public UserDto User { get; set; }
    }
}
