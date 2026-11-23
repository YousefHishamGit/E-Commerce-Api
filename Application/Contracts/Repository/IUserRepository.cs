using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts.Repository
{
    public interface IUserRepository : IGenericRepository<User,Guid>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> PhoneExistsAsync(string phone, Guid? excludeUserId = null);

    }
}
