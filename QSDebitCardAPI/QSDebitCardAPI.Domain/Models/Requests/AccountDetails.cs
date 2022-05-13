using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QSDataUpdateAPI.Domain.Models.Requests
{
    public class AccountDetailsRequest
    {
        [Required(ErrorMessage = "Bank Account Number is required")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Invalid Account Number")]
        public string AccountNumber { get; set; }

        [Required(ErrorMessage = "Please provide the phone number tied to specified account")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Invalid Phone Number")]
        public string PhoneNumber { get; set; }
    }

    public class AccountEnquiryRequest
    {
        [Required(ErrorMessage = "Bank Account Number is required")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Invalid Account Number")]
        public string AccountNumber { get; set; }
    }

    public class TinEnquiryRequest
    {
        [Required(ErrorMessage = "Tin Number is required")]
        public string TinNumber { get; set; }

        public string AccountName { get; set; }
    }

    public class ValidateRefereeOtpRequest
    {
        [Required(ErrorMessage = "Bank Account Number is required")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Invalid Account Number")]
        public string AccountNumber { get; set; }

        public string Otp { get; set; }
        public string OtpSourceReference { get; set; }
    }
}