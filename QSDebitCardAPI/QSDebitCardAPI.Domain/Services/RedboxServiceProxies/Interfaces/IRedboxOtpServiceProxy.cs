using System;
using System.Threading.Tasks;
using QSDataUpdateAPI.Domain.Models.Requests.Redbox;

namespace QSDataUpdateAPI.Core.Interfaces.Services.Helpers.Redbox
{
    public interface IRedboxOtpServiceProxy
    {
        Task<(string responseCode, string responseDescription)> InitiateOtp(RedboxOtpRequest request);
        Task<(string responseCode, string responseDescription)> VerifyOtp(RedboxOtpVerificationRequest request);
        Task<(string responseCode, string responseDescription, string otpReference)> InitiateOtpReqManager(string accountNumber);
        Task<(string responseCode, string responseDescription)> VerifyOtpReqManager(string accountNumber, string otp, string sourceReference);
        Task<(string responseCode, string responseDescription)> UpdatePhoneAndEmailOnFinacle(string accountNumber, string phoneNumber, string email);
    }
}
