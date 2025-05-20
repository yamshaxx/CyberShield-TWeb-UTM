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
        private readonly IBlogService _blogService;
        private readonly IUserService _userService;
        private readonly IAppointmentService _appointmentService;
        private readonly IAdminService _adminService;
        private readonly IValidationService _validationService;
        private readonly IErrorHandlingService _errorHandlingService;
        private readonly IContactMessageService _contactMessageService;
        private readonly IServiciiService _serviciiService;
        private readonly IHelpService _helpService;
        private readonly IDespreService _despreService;
        private readonly ITestService _testService;

        public BusinessLogic()
        {
            _errorHandlingService = new ErrorHandlingService();
            _validationService = new ValidationService();
            _authBL = new BL_Struct.AuthBL(_errorHandlingService);
            _blogService = new BlogService(_errorHandlingService, _validationService);
            _userService = new UserService(_errorHandlingService);
            _appointmentService = new AppointmentService(_errorHandlingService, _validationService);
            _adminService = new AdminService(_errorHandlingService, _authBL);
            _contactMessageService = new ContactMessageService();
            _serviciiService = new ServiciiService(_errorHandlingService, _validationService);
            _helpService = new HelpService(_errorHandlingService);
            _despreService = new DespreService(_errorHandlingService);
            _testService = new TestService(_errorHandlingService);
        }

        public BusinessLogic(INetworkPentestRepository networkPentestRepository, IClientRepository clientRepository)
        {
            _errorHandlingService = new ErrorHandlingService();
            _validationService = new ValidationService();
            _authBL = new BL_Struct.AuthBL(_errorHandlingService);
            _blogService = new BlogService(_errorHandlingService, _validationService);
            _userService = new UserService(_errorHandlingService);
            _appointmentService = new AppointmentService(_errorHandlingService, _validationService);
            _adminService = new AdminService(_errorHandlingService, _authBL);
            _contactMessageService = new ContactMessageService();
            _serviciiService = new ServiciiService(_errorHandlingService, _validationService);
            _helpService = new HelpService(_errorHandlingService);
            _despreService = new DespreService(_errorHandlingService);
            _testService = new TestService(_errorHandlingService);
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
        
        public IBlogService GetBlogService()
        {
            return _blogService;
        }
        
        public IUserService GetUserService()
        {
            return _userService;
        }
        
        public IAppointmentService GetAppointmentService()
        {
            return _appointmentService;
        }
        
        public IAdminService GetAdminService()
        {
            return _adminService;
        }
        
        public IValidationService GetValidationService()
        {
            return _validationService;
        }
        
        public IErrorHandlingService GetErrorHandlingService()
        {
            return _errorHandlingService;
        }
        
        public IContactMessageService GetContactMessageService()
        {
            return _contactMessageService;
        }
        
        public IServiciiService GetServiciiService()
        {
            return _serviciiService;
        }
        
        public IHelpService GetHelpService()
        {
            return _helpService;
        }
        
        public IDespreService GetDespreService()
        {
            return _despreService;
        }
        
        public ITestService GetTestService()
        {
            return _testService;
        }
    }
}
