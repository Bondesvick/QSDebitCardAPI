using System;
using System.Collections.Generic;

namespace QSDataUpdateAPI.Core.Domain.Entities
{
    public partial class CustomerRequest
    {
        public CustomerRequest()
        {
            DebitCardDetails = new HashSet<DebitCardDetails>();
        }

        public long Id { get; set; }
        public string TranId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string CustomerAuthType { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string TreatedBy { get; set; }
        public DateTime? TreatedDate { get; set; }
        public string RequestType { get; set; }
        public string TreatedByUnit { get; set; }
        public string RejectionReason { get; set; }
        public string Remarks { get; set; }
        public string Bvn { get; set; }

        //public string RejectionType { get; set; }

        public virtual ICollection<DebitCardDetails> DebitCardDetails { get; set; }
    }
}
