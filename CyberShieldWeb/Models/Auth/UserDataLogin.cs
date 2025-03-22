using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CyberShieldWeb.Models.Auth
{
    public class UserDataLogin
    {
        public string Password { get; internal set; }
        public string UserName { get; internal set; }
    }
}