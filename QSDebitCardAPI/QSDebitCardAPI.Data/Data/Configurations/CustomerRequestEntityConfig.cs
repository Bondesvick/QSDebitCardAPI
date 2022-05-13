using QSDataUpdateAPI.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace QSDataUpdateAPI.Data.Data.Configurations
{
    public class CustomerRequestEntityConfig : IEntityTypeConfiguration<CustomerRequest>
    {
        public void Configure(EntityTypeBuilder<CustomerRequest> builder)
        {
            builder.ToTable("CUSTOMER_REQUEST");

            builder.HasIndex(e => e.Status)
                .HasName("IX_REQUEST_STATUS");

            builder.HasIndex(e => e.TreatedByUnit)
                .HasName("IX_TRBY_UNIT");

            builder.Property(e => e.Id).HasColumnName("ID");

            builder.Property(e => e.AccountName)
                .HasColumnName("ACCOUNT_NAME")
                .HasMaxLength(500);

            builder.Property(e => e.AccountNumber)
                .HasColumnName("ACCOUNT_NUMBER")
                .HasMaxLength(10);

            builder.Property(e => e.CreatedDate).HasColumnName("CREATED_DATE");

            builder.Property(e => e.CustomerAuthType)
                .IsRequired()
                .HasColumnName("CUSTOMER_AUTH_TYPE")
                .HasMaxLength(20);

            builder.Property(e => e.RejectionReason)
                .HasColumnName("REJECTION_REASON")
                .HasMaxLength(500);

            builder.Property(e => e.Remarks)
                .HasColumnName("REMARKS")
                .HasMaxLength(500);

            builder.Property(e => e.RequestType)
                .IsRequired()
                .HasColumnName("REQUEST_TYPE")
                .HasMaxLength(200);

            builder.Property(e => e.Status)
                .IsRequired()
                .HasColumnName("STATUS")
                .HasMaxLength(50);

            builder.Property(e => e.TranId)
                .HasColumnName("TRAN_ID")
                .HasMaxLength(20);

            builder.Property(e => e.TreatedBy)
                .HasColumnName("TREATED_BY")
                .HasMaxLength(20);

            builder.Property(e => e.TreatedByUnit)
                .HasColumnName("TREATED_BY_UNIT")
                .HasMaxLength(100);

            builder.Property(e => e.Bvn)
                .HasColumnName("BVN")
                .HasMaxLength(100);

            builder.Property(e => e.TreatedDate).HasColumnName("TREATED_DATE");
        }
    }
}
