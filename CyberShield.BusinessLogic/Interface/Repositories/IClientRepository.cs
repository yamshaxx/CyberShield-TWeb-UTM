using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CyberShield.Domain.Model.Client;

namespace CyberShield.BusinessLogic.Interface.Repositories
{
    public interface IClientRepository
    {
        Task<ClientDTO> GetByIdAsync(int id);
        Task<ClientDTO> GetByEmailAsync(string email);
        Task<int> CreateAsync(ClientDTO client);
        Task UpdateAsync(ClientDTO client);
    }
}
