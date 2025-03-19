using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CyberShield.BusinessLogic.Core;
using CyberShield.BusinessLogic.Interface;
using CyberShield.Domain.Model.User;

namespace CyberShield.BusinessLogic.BL_Struct
{
    public class AuthBL : UserApi, IAuth
    {
        public string UserAuthLogic(UserLoginDTO data)
        {
            throw new NotImplementedException();
        }
    }
}
