using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QSDataUpdateAPI.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace QSDataUpdateAPI.Data.Data.Configurations
{
    public class AuditConfiguration : IEntityTypeConfiguration<Audit>
    {
        public void Configure(EntityTypeBuilder<Audit> builder)
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            builder.Property(t => t.ActionBy).HasColumnName("ACTIONBY").IsRequired().HasMaxLength(50);
            builder.Property(t => t.ActionDescription).HasColumnName("ACTIONDESCRIPTION").IsRequired().HasMaxLength(600);
            builder.Property(t => t.ComputerName).HasColumnName("COMPUTERNAME").IsRequired().HasMaxLength(150);
            builder.Property(t => t.IPAddress).HasColumnName("IPADDRESS").IsRequired().HasMaxLength(20);
            builder.Property(t => t.Method).HasColumnName("METHOD").IsRequired().HasMaxLength(50);
            builder.Property(t => t.RequestType).HasColumnName("REQUESTTYPE").IsRequired().HasMaxLength(50);
            builder.Property(t => t.AuditDateTime).HasColumnName("AUDITDATETIME").IsRequired();
            builder.Property(t => t.Hash).HasColumnName("HASH").IsRequired();

            builder.ToTable("AUDIT_DETAIL", "dbo");
        }
    }
}
