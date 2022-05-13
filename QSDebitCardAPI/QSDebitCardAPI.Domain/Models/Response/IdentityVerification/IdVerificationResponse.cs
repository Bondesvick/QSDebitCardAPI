using System;
using System.Collections.Generic;
using System.Text;

namespace QSDataUpdateAPI.Domain.Models.Response.IdentityVerification
{
    public class IdVerificationResponse
    {
        public string sec_key { get; set; }
        public string timestamp { get; set; }
        public string JSONVersion { get; set; }
        public string SmileJobID { get; set; }
        public string ResultType { get; set; }
        public string ResultText { get; set; }
        public int ResultCode { get; set; }
        public string Country { get; set; }
        public string IDType { get; set; }
        public string IDNumber { get; set; }
        public string ExpirationDate { get; set; }
        public string Photo { get; set; }
        public string IsFinalResult { get; set; }
        public PartnerParams PartnerParams { get; set; }
        public SmileIDActions Actions { get; set; }
        public string FullName { get; set; }
        public string DOB { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string Source { get; set; }
        public SmileIDFullData FullData { get; set; }

    }

    public class SmileIDFullData
    {
        public string MyProperty { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string gender { get; set; }
        public string authorizationCode { get; set; }
        public string imageUrl { get; set; }
        public string dateOfBirth { get; set; }
        public string otherName { get; set; }
        public string issuedAt { get; set; }
        public string idNumber { get; set; }
        public string issueDate { get; set; }
        public string type { get; set; }
        public string success { get; set; }
    }

    public class SmileIDActions
    {
        public string Verify_ID_Number { get; set; }
        public string Return_Personal_Info { get; set; }

    }

    public class PartnerParams
    {
        public string job_id { get; set; }
        public string user_id { get; set; }
        public int job_type { get; set; }

    }
}
