using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QSDataUpdateAPI.Domain.Models.Requests
{
    public class DebitCardRequest
    {
        public string PhoneNumber { get; set; }
        public string AuthType { get; set; }
        public List<DataUpdateDocumentModel> Documents { get; set; }
        public string OtpSourceReference { get; set; }
        public string Otp { get; set; }
        public string OtpIdentifier { get; set; }
        public string OtpReasonCode { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string BVN { get; set; }
        public string hotlistCode { get; set; }

        // Terms and Conditions
        public bool IAcceptTermsAndCondition { get; set; }

        public string DateOfAcceptingTAndC { get; set; }

        public string DateOfBirth { get; set; }
        public string RequestType { get; set; }
        public string NameOnCard { get; set; }

        //public string CardType { get; set; }
        public string Branch { get; set; }

        public string AccountToDebit { get; set; }
        public string hotlistedCard { get; set; }
        public string AccountStatus { get; set; }

        public string CaseId { get; set; }
        public string CurrentStep { get; set; }
        public bool Submitted { get; set; } = false;

        //public string CardType { get; set; }

        //public string Gender { get; set; }
        //public string Title { get; set; }
        //public string MaritalStatus { get; set; }
        //public string City { get; set; }

        public string expiryDate { get; set; }
    }

    public class DataUpdateDocumentModel
    {
        public string Title { get; set; }

        [Required(ErrorMessage = "Document name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Base 64 encoded document content is required")]
        public string Base64Content { get; set; }

        public string ContentType { get; set; }
    }
}