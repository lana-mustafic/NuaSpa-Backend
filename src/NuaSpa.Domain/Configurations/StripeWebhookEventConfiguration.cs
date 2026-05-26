using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Domain.Configurations;

public class StripeWebhookEventConfiguration : IEntityTypeConfiguration<StripeWebhookEvent>
{
    public void Configure(EntityTypeBuilder<StripeWebhookEvent> builder)
    {
        builder.ToTable("StripeWebhookEvents");

        builder.HasIndex(x => x.StripeEventId).IsUnique();
        builder.Property(x => x.StripeEventId).HasMaxLength(255).IsRequired();
        builder.Property(x => x.EventType).HasMaxLength(100).IsRequired();
    }
}
