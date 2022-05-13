using System.Collections.Generic;
using System.Threading.Tasks;
using QSDataUpdateAPI.Core.Domain.Entities;
using QSDataUpdateAPI.Domain.Models.Requests;
using QSDataUpdateAPI.Domain.Models.Response;
using QSDataUpdateAPI.Domain.Models.Response.Redbox;
using QSDebitCardAPI.Data.Data.Entities;
using QSDebitCardAPI.Domain.Models.Response.Redbox;

namespace QSDebitCardAPI.Domain.Services.RedboxServiceProxies.Interfaces
{
    public interface IRedboxAccountServiceProxy
    {
        Task<(string responseCode, string responseDescription, CustomerAccountInfo result)> GetCustomerAccountDetails(string accountNumber, string phoneNumber);

        Task<(string responseCode, string responseDescription, string segment)> GetAccountSegment(string accountNumber);

        Task<(string responseCode, string responseDescription, CustomerAccountInfo result)> DoCorporateAccountEnquiry(string accountNumber);

        Task<(string responseCode, string responseDescription, AccountEnquiryInfo result)> DoAccountCIFEnquiry(string accountNumber);

        Task<(string responseCode, string responseDescription, CustomerAccountInfo result)> DoBVNEnquiry(string bvnId);

        //Task<(string responseCode, string responseDescription, TinEnquiryInfo result)> DoTinEnquiry(string tinNumber);
        Task<(string responseCode, string responseDescription, ActiveCardEnquiryInfo result)> GetActiveCard(string accountNumber);

        Task<(string responseCode, string responseDescription, CardTypeResponse.CardPrograms result)> GetCardType(
            string accountNumber);

        Task<(bool, LoanRepaymentDetailDTO, string)> AccountSchemeEnquiry(string accountNumber);

        Task<(string responseCode, string responseDescription)> SubmitDebitCardEnquiry(DebitCardRequest debitCardDetails, AccountEnquiryInfo cifInfo);

        Task<(string responseCode, string responseDescription, CardTypes result)> DebitCardTypes(string accountNumber);

        Task<(string responseCode, string responseDescription)> DoHotlistCardEnquiry(string accountNumber, string maskPan, string hotlistCode, string exp);

        Task<List<CityState>> GetCityState();

        Task<List<BankBranch>> GetBranches();
    }
}