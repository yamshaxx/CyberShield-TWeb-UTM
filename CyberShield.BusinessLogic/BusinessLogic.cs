using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CyberShield.BusinessLogic.BL_Struct;
using CyberShield.BusinessLogic.BL_Struct.PenetrationTesting;
using CyberShield.BusinessLogic.Interface;
using CyberShield.BusinessLogic.Interface.Repositories;

namespace CyberShield.BusinessLogic
{
    public class BusinessLogic
    {
        private readonly IAuth _authBL;
        private readonly INetworkPentestService _networkPentestService;

        public BusinessLogic(INetworkPentestRepository networkPentestRepository, IClientRepository clientRepository)
        {
            _authBL = new AuthBL();
            _networkPentestService = new NetworkPentestService(clientRepository, networkPentestRepository);
        }

        public IAuth GetAuthBL()
        {
            return _authBL;
        }

        public INetworkPentestService GetNetworkPentestService()
        {
            return _networkPentestService;
        }
    }
}
