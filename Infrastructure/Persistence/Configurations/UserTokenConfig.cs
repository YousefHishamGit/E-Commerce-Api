using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class UserTokenConfig : IEntityTypeConfiguration<UserToken>
    {
        public void Configure(EntityTypeBuilder<UserToken> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Type)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(t => t.TokenHash)
                .IsRequired()
                .HasMaxLength(64);                 // SHA-256 hex

            builder.HasIndex(t => t.TokenHash).IsUnique();
            builder.HasIndex(t => new { t.UserId, t.Type });

            builder.Ignore(t => t.IsActive);

            builder.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
