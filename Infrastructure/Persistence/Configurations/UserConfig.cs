using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Configurations
{
    public class UserConfig : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.Phone)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(u => u.Phone)
                .IsUnique(); 

            builder.Property(u => u.Gender)
                .HasConversion<string>(); 

            builder.Property(u => u.Role)
                .HasConversion<string>()
                .IsRequired();

        
            builder.HasMany(u => u.Addresses)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(u => u.Wishlist)
                .WithOne()
                .HasForeignKey<Wishlist>("UserId") 
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
