using CoreLedger.Domain.Transfers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Config;

public sealed class TransferConfig: IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> b)
    {
        b.ToTable("transfers");
        b.HasKey(x => x.TransferId);

        b.Property(x => x.TransferId).HasColumnName("transfer_id");
        b.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(16).IsRequired();
    }
}