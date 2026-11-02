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
    public class OfferConfig : IEntityTypeConfiguration<Offer>
    {
        public void Configure(EntityTypeBuilder<Offer> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Title)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(o => o.Description)
                .HasMaxLength(500);

            builder.Property(o => o.DiscountPercentage)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            builder.Property(o => o.StartDate)
                .IsRequired();

            builder.Property(o => o.EndDate)
                .IsRequired();

            builder.Property(o => o.IsActive)
                .HasDefaultValue(true);

            builder.HasOne(o => o.Product)
                .WithMany()
                .HasForeignKey(o => o.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(o => o.Category)
                .WithMany()
                .HasForeignKey(o => o.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
