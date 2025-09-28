using CoreLedger.Domain.Ledger;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Config;

public sealed class LedgerEntryConfig : IEntityTypeConfiguration<LedgerEntry>
{
    public void Configure(EntityTypeBuilder<LedgerEntry> b)
    {
        b.ToTable("ledger_entries");
        b.HasKey(x => x.EntryId);

        b.Property(x => x.EntryId).HasColumnName("entry_id");
        b.Property(x => x.AccountId).HasColumnName("account_id").IsRequired();
        b.Property(x => x.TransferId).HasColumnName("transfer_id").IsRequired();
        
        b.ComplexProperty(x => x.Amount, money =>
        {
            money.Property(p => p.Amount)
                .HasColumnName("amount")
                .HasPrecision(19, 4)
                .IsRequired();

            money.Property(p => p.Currency)
                .HasColumnName("currency")
                .HasMaxLength(16)
                .IsRequired();
        });

        b.Property(x => x.BookingDate).HasColumnName("booking_date");
        b.Property(x => x.ValueDate).HasColumnName("value_date");

        b.Property(x => x.Direction)
            .HasColumnName("direction")
            .HasConversion<int>();

        b.HasIndex(x => x.TransferId).HasDatabaseName("ix_ledger_entries_transfer");
        b.HasIndex(x => x.AccountId).HasDatabaseName("ix_ledger_entries_account");
    }
}