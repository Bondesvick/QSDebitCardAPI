using QSDataUpdateAPI.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace QSDataUpdateAPI.Data.Data.Configurations
{
    public class DebitCardDetailsEntityConfig : IEntityTypeConfiguration<DebitCardDetails>
    {
        public void Configure(EntityTypeBuilder<DebitCardDetails> entity)
        {
            entity.ToTable("DEBIT_CARD_DETAILS");

            entity.Property(e => e.Id).HasColumnName("ID");

            entity.Property(e => e.CustomerReqId).HasColumnName("CUSTOMER_REQ_ID")
                .IsRequired();

            entity.Property(e => e.PhoneNumber)
                .HasColumnName("PHONE_NUMBER")
                .HasMaxLength(20);

            entity.Property(e => e.BVN)
                .HasColumnName("BVN")
                .HasMaxLength(50);

            entity.Property(e => e.AccountStatus)
                .HasColumnName("ACCOUNT_STATUS")
                .HasMaxLength(50);

            entity.Property(e => e.AccountToDebit)
                .HasColumnName("ACCOUNT_TO_DEBIT")
                .HasMaxLength(50);

            entity.Property(e => e.Branch)
                .HasColumnName("BRANCH")
                .HasMaxLength(50);

            entity.Property(e => e.City)
                .HasColumnName("CITY")
                .HasMaxLength(50);

            entity.Property(e => e.Title)
                .HasColumnName("TITLE")
                .HasMaxLength(50);

            entity.Property(e => e.Gender)
                .HasColumnName("GENDER")
                .HasMaxLength(50);

            entity.Property(e => e.MaritalStatus)
                .HasColumnName("MARITAL_STATUS")
                .HasMaxLength(50);

            entity.Property(e => e.DateOfBirth)
                .HasColumnName("DATE_OF_BIRTH")
                .HasMaxLength(50);

            entity.Property(e => e.hotlistCode)
                .HasColumnName("HOTLIST_CODE")
                .HasMaxLength(50);

            entity.Property(e => e.hotlistedCard)
                .HasColumnName("HOTLISTED_CARD")
                .HasMaxLength(50);

            entity.Property(e => e.NameOnCard)
                .HasColumnName("NAME_ON_CARD")
                .HasMaxLength(50);

            entity.Property(e => e.RequestType)
                .HasColumnName("REQUEST_TYPE")
                .HasMaxLength(50);

            // Session
            entity.Property(e => e.CaseId)
                .HasColumnName("CASE_ID")
                .HasMaxLength(100);

            entity.Property(e => e.CurrentStep)
                .HasColumnName("CURRENT_STEP")
                .HasMaxLength(100);

            entity.Property(e => e.Submitted)
                .HasColumnName("SUBMITTED")
                .HasMaxLength(100);

            // Terms and Conditions
            entity.Property(e => e.IAcceptTermsAndCondition)
                .HasColumnName("I_ACCEPT_TERMS_AND_CONDITIONS")
                .HasMaxLength(100);

            entity.Property(e => e.DateOfAcceptingTAndC)
                .HasColumnName("DATE_OF_ACCEPTING_T_AND_C")
                .HasMaxLength(100);

            entity.HasOne(d => d.CustomerReq)
                .WithMany(p => p.DebitCardDetails)
                .HasForeignKey(d => d.CustomerReqId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DEBIT_CARD_REQUEST_DETAILS");
        }
    }
}