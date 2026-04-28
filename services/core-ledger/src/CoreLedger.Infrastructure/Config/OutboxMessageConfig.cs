using CoreLedger.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Config;

public sealed class OutboxMessageConfig : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> b)
    {
        b.ToTable("outbox_messages");

        b.HasKey(x => x.Id);

        b.Property(x => x.Id)
            .HasColumnName("id");

        b.Property(x => x.Type)
            .HasColumnName("type")
            .HasMaxLength(256)
            .IsRequired();

        b.Property(x => x.Payload)
            .HasColumnName("payload")
            .HasColumnType("jsonb")
            .IsRequired();

        b.Property(x => x.OccurredAtUtc)
            .HasColumnName("occurred_at_utc")
            .IsRequired();

        b.Property(x => x.ProcessedAtUtc)
            .HasColumnName("processed_at_utc");

        b.Property(x => x.Attempts)
            .HasColumnName("attempts")
            .IsRequired();

        b.Property(x => x.LastError)
            .HasColumnName("last_error")
            .HasMaxLength(2048);

        b.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        b.HasIndex(x => new { x.Status, x.OccurredAtUtc })
            .HasDatabaseName("ix_outbox_messages_status_occurred_at");
    }
}