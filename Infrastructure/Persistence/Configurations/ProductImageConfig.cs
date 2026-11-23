using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ProductImageConfig : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.Url)
                .IsRequired()
                .HasMaxLength(500);

            // One image per slot per product + slots limited to 0..3 => max 4 images, enforced by the DB
            builder.HasIndex(i => new { i.ProductId, i.SortOrder }).IsUnique();
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_ProductImages_SortOrder",
                $"[SortOrder] >= 0 AND [SortOrder] < {ProductImage.MaxPerProduct}"));

            builder.HasOne(i => i.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
