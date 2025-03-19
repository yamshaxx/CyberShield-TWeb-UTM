using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CyberShield.BusinessLogic.BL_Struct;
using CyberShield.BusinessLogic.Interface;

namespace CyberShield.BusinessLogic
{
    public class BusinessLogic
    {
        private readonly IAuth _authBL;

        public BusinessLogic()
        {
            _authBL = new AuthBL();
        }

        public IAuth GetAuthBL()
        {
            return _authBL;
        }
    }
}
