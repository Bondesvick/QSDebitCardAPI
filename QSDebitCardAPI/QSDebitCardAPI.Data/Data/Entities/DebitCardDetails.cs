using System;
using System.Collections.Generic;

namespace QSDataUpdateAPI.Core.Domain.Entities
{
    public partial class DebitCardDetails
    {
        public int Id { get; set; }
        public long CustomerReqId { get; set; }
        public virtual CustomerRequest CustomerReq { get; set; }
        public virtual ICollection<DebitCardDocument> Documents { get; set; }

        // Account Info
        public string AccountStatus { get; set; }

        public string AuthType { get; set; }
        public string BVN { get; set; }
        public string PhoneNumber { get; set; }

        // Data

        public string DateOfBirth { get; set; }
        public string RequestType { get; set; }
        public string NameOnCard { get; set; }
        public string Branch { get; set; }
        public string City { get; set; }
        public string Title { get; set; }
        public string Gender { get; set; }
        public string MaritalStatus { get; set; }
        public string AccountToDebit { get; set; }
        public string hotlistedCard { get; set; }
        public string hotlistCode { get; set; }

        public string CardType { get; set; }

        // Session
        public string CaseId { get; set; }

        public string CurrentStep { get; set; }
        public bool Submitted { get; set; }

        // Terms and Conditions
        public bool IAcceptTermsAndCondition { get; set; }

        public DateTime DateOfAcceptingTAndC { get; set; }

        public DebitCardDetails()
        {
            Documents ??= new List<DebitCardDocument>();
        }
    }
}