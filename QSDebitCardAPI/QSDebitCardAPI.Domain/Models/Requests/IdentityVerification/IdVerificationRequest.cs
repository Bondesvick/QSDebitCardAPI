using System;
using System.Collections.Generic;
using System.Text;

namespace QSDataUpdateAPI.Domain.Models.Requests.IdentityVerification
{
    public class IdVerificationRequest
    {
        public string country { get; set; } = "NG";
        public string idType { get; set; }
        public string idNumber { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string phoneNumber { get; set; }
        public string dob { get; set; }
        public string channel { get; set; } = "MOBILE_APP";
        public string moduleId { get; set; }
    }
}
