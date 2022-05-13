using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QSDebitCardAPI.Data.Data.Entities;

namespace QSDebitCardAPI.Data.Data.Configurations
{
    public class BankBranchConfiguration : IEntityTypeConfiguration<BankBranch>
    {
        public void Configure(EntityTypeBuilder<BankBranch> builder)
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasColumnName("ID").ValueGeneratedOnAdd();

            builder.Property(t => t.sol_Id).HasColumnName("SOL_ID");
            builder.Property(t => t.branch).HasColumnName("BRANCH").HasMaxLength(100);
            builder.Property(t => t.email_Address).HasColumnName("EMAIL_ADDRESS").HasMaxLength(50);

            builder.ToTable("BANK_BRANCHES", "dbo");
        }
    }
}