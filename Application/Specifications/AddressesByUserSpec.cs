using Domain.Entities;
using System;

namespace Application.Specifications
{
    public class AddressesByUserSpec : BaseSpecification<Address>
    {
        // 1. All addresses for a specific user
        public AddressesByUserSpec(Guid userId)
            : base(a => a.UserId == userId)
        {
            AddOrderByDescending(a => a.CreatedAt);
        }

        // 2. Specific address belonging to a specific user (Security check)
        public AddressesByUserSpec(int addressId, Guid userId)
            : base(a => a.Id == addressId && a.UserId == userId)
        {
        }
    }
}

