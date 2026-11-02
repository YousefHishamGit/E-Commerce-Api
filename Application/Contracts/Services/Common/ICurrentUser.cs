using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts.Services.Common
{
    public interface ICurrentUser
    {
        public Guid UserId { get; }
        public UserRole Role { get; }
    }
}
