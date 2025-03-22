using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CyberShield.Domain.Model.User;

namespace CyberShield.BusinessLogic.Interface
{
    public interface IAuth
    {
        string UserAuthLogic(UserLoginDTO data);
    }
}
