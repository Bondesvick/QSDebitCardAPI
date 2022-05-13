using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using QSDataUpdateAPI.Data;
using QSDataUpdateAPI.Core.Domain.Entities;
using QSDataUpdateAPI.Data.Data.Configurations;
using Microsoft.Extensions.Configuration;
using QSDataUpdateAPI.Domain.Services.Helpers;
using QSDebitCardAPI.Data.Data.Configurations;
using QSDebitCardAPI.Data.Data.Entities;

namespace QSDataUpdateAPI.Data.Data
{
    public partial class QuickServiceDbContext : DbContext
    {
        public QuickServiceDbContext()
        {
        }

        public QuickServiceDbContext(DbContextOptions<QuickServiceDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<DebitCardDetails> AddAccOpeningDetails { get; set; }
        public virtual DbSet<DebitCardDocument> AddAccOpeningDocs { get; set; }
        public virtual DbSet<CustomerRequest> CustomerRequest { get; set; }
        public virtual DbSet<BankBranch> BankBranch { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new DebitCardDocumentEntityConfig());
            modelBuilder.ApplyConfiguration(new DebitCardDetailsEntityConfig());
            modelBuilder.ApplyConfiguration(new CustomerRequestEntityConfig());
            modelBuilder.ApplyConfiguration(new AuditConfiguration());
            modelBuilder.ApplyConfiguration(new BankBranchConfiguration());
            OnModelCreatingPartial(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //if (!optionsBuilder.IsConfigured)
            //{
            //    optionsBuilder.UseSqlServer(@"Data Source=52.166.249.18;Initial Catalog=QuickService;User Id=sa;Password=sql@Passw0rd123;");
            //}

            if (!optionsBuilder.IsConfigured)
            {
                var connStr = ServiceResolver.Resolve<IConfiguration>().GetConnectionString("QuickServiceDbConn");
                optionsBuilder.UseSqlServer(connStr).EnableSensitiveDataLogging();
                base.OnConfiguring(optionsBuilder);
            }
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}