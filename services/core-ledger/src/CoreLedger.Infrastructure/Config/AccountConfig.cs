using CoreLedger.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Config;

public sealed class AccountConfig: IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> b)
    {
        b.ToTable("accounts");
        b.HasKey(x => x.AccountId);

        b.Property(x => x.AccountId).HasColumnName("account_id");
        b.Property(x => x.CustomerId).HasColumnName("customer_id").IsRequired();
        b.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(16).IsRequired();
        b.Property(x => x.Status).HasColumnName("status").HasConversion<int>();

        b.HasIndex(x => x.CustomerId).HasDatabaseName("ix_accounts_customer");
    }
}