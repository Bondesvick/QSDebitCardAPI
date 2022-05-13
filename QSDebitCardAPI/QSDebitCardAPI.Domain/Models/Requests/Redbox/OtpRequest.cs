using System;
using System.ComponentModel.DataAnnotations;

namespace QSDataUpdateAPI.Domain.Models.Requests.Redbox
{
    public class RedboxOtpRequest
    {
        // [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; }
        public string CifId { get; set; }
        public string ReasonCode { get; set; }
        public string ReasonDescription { get; set; }
    }

    public class RedboxOtpVerificationRequest
    {
        [Required(ErrorMessage = "Otp source reference is required")]
        public string OtpSourceReference { get; set; }
        [Required(ErrorMessage = "CifId is required")]
        public string CifId { get; set; }
        [Required(ErrorMessage = "Otp is required")]
        public string Otp { get; set; }

        public RedboxOtpVerificationRequest() { }
        public RedboxOtpVerificationRequest(string otpSourceRef, string cifId)
        {
            OtpSourceReference = otpSourceRef;
            CifId = cifId;
        }
    }
}
