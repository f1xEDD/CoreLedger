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
        
        b.UsePropertyAccessMode(PropertyAccessMode.Field);
        
        b.Property<Guid>("_entryId").HasColumnName("entry_id");
        b.Property<Guid>("_accountId").HasColumnName("account_id").IsRequired();
        b.Property<Guid>("_transferId").HasColumnName("transfer_id").IsRequired();
        
        b.ComplexProperty(x => x.Amount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("amount").HasPrecision(19, 4).IsRequired();
            money.Property(m => m.Currency).HasColumnName("currency").HasMaxLength(16).IsRequired();
        });

        b.Property<DateOnly>("_bookingDate").HasColumnName("booking_date");
        b.Property<DateOnly>("_valueDate").HasColumnName("value_date");

        b.Property(x => x.Direction).HasColumnName("direction").HasConversion<int>();

        b.HasIndex("TransferId").HasDatabaseName("ix_ledger_entries_transfer");
        b.HasIndex("AccountId").HasDatabaseName("ix_ledger_entries_account");
    }
}