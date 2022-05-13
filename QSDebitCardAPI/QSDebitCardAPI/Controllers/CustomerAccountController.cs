using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QSDataUpdateAPI.Core.Domain.Entities;
using QSDataUpdateAPI.Core.Interfaces.Services.Helpers.Redbox;
using QSDataUpdateAPI.Domain.Models;
using QSDataUpdateAPI.Domain.Models.Requests;
using QSDataUpdateAPI.Domain.Models.Requests.IdentityVerification;
using QSDataUpdateAPI.Domain.Models.Response;
using QSDataUpdateAPI.Domain.Models.Response.IdentityVerification;
using QSDataUpdateAPI.Domain.Models.Response.Redbox;
using QSDataUpdateAPI.Domain.Services.Helpers;
using QSDataUpdateAPI.Domain.Services.Interfaces;
using QSDataUpdateAPI.Filters;
using QSDebitCardAPI.Data.Data.Entities;
using QSDebitCardAPI.Domain.Models.Response.Redbox;
using QSDebitCardAPI.Domain.Services.RedboxServiceProxies.Interfaces;
using QSDebitCardAPI.Filters;

namespace QSDebitCardAPI.Controllers
{
    //[ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(AuthSecretKeyFilter), Order = 1)]
    public class CustomerAccountController : Controller
    {
        private readonly IRedboxAccountServiceProxy _customerAccountsService;
        private readonly IRedboxOtpServiceProxy _otpServiceProxy;
        private readonly IIdVerificationService _idVerificationService;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<CustomerAccountController> _logger;

        public CustomerAccountController(ILogger<CustomerAccountController> logger, IRedboxAccountServiceProxy customerAccountsService, IIdVerificationService idVerificationService, IAuditLogService auditLogService)
        {
            _logger = logger;
            _customerAccountsService = customerAccountsService;
            _idVerificationService = idVerificationService;
            _auditLogService = auditLogService;
        }

        [HttpPost("accountDetails")]
        public async Task<IActionResult> GetAccountDetails([FromBody] AccountDetailsRequest request)
        {
            try
            {
                await _auditLogService.AuditLog(request.AccountNumber, "Debit Card", "GetAccountDetails", "Commencing the validation of Customer account details: ", HttpContext.Connection.RemoteIpAddress.ToString(), Dns.GetHostName().ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception occured on api/customeraccounts/accountDetails -> {ex.Message}", ex);
                return Ok(new ApiResponse<object>(ResponseCodeConstants.INTERNAL_EXCEPTION, ex.Message));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var accounEnquiryTask = _customerAccountsService.DoAccountCIFEnquiry(request.AccountNumber);
                var accountSegmentTask = _customerAccountsService.GetAccountSegment(request.AccountNumber);
                var cardTypeTask = _customerAccountsService.DebitCardTypes(request.AccountNumber);

                await Task.WhenAll(accountSegmentTask, accounEnquiryTask, cardTypeTask);

                var accountSegmentResponse = accountSegmentTask.Result;
                var accounEnquiryResponse = accounEnquiryTask.Result;
                var cardTypeResponse = cardTypeTask.Result;

                if (accountSegmentResponse.responseCode != "00" && accountSegmentResponse.responseCode != "000")
                    return Ok(new ApiResponse<object>(accountSegmentResponse.responseCode, $"Failed to fetch customer segment for account {request.AccountNumber}"));
                if (accounEnquiryResponse.responseCode != "00")
                    return Ok(new ApiResponse<object>(accounEnquiryResponse.responseCode, $"Failed to fetch customer details for {request.AccountNumber} at this time. {accounEnquiryResponse.responseDescription}"));

                var phoneMatches = DoPhoneNumberValidation(request.PhoneNumber, accounEnquiryResponse.result.PhoneNumber);

                if (!phoneMatches)
                    return Ok(new ApiResponse<object>("01", $"{request.PhoneNumber} does not match the phone number on the account. please check and retry"));

                var schemeCode = accounEnquiryResponse.result.AccountSchemeCode;
                var schemeType = accounEnquiryResponse.result.AccountSchemeType;
                var currencyCode = accounEnquiryResponse.result.AccountCurrencyCode;

                if (
                    schemeCode == "OD005" || schemeCode == "OD006" ||
                    schemeCode == "ODVIS" || schemeType == "LAA" || currencyCode != "NGN")
                {
                    return Ok(new ApiResponse<object>(ResponseCodeConstants.FAILURE, $"You cannot request for debit card with the account {request.AccountNumber}. Please contact the customer contact centre on 0700 909 909 909 or Customercarenigeria@stanbicibtc.com or visit any of our branches nationwide."));
                }

                if (cardTypeResponse.responseDescription.Contains("Customer already has an active card"))
                {
                    return Ok(new ApiResponse<CustomerAccountInfo>
                    {
                        ResponseCode = "03",
                        ResponseDescription = accounEnquiryResponse.responseDescription,
                        Data = MapToCustomerInfo(accounEnquiryResponse.result, accountSegmentResponse.segment, cardTypeResponse.result)
                    });
                }

                var result = new ApiResponse<CustomerAccountInfo>
                {
                    ResponseCode = "00",
                    ResponseDescription = accounEnquiryResponse.responseDescription,
                    Data = MapToCustomerInfo(accounEnquiryResponse.result, accountSegmentResponse.segment, cardTypeResponse.result)
                };
                return Ok(result);
            }
            catch (Exception exception)
            {
                _logger.LogError($"Exception occured on api/customeraccounts/accountDetails -> {exception.Message}", exception);
                return Ok(new ApiResponse<object>(ResponseCodeConstants.INTERNAL_EXCEPTION, exception.Message));
            }
        }

        [HttpPost("validateIdCard")]
        [ProducesResponseType(200, Type = typeof(ApiResponse<SmileIDFullData>))]
        public async Task<IActionResult> ValidateIdCard([FromBody] IdVerificationRequest request)
        {
            try
            {
                await _auditLogService.AuditLog(request.phoneNumber, "Debit Card", "ValidateIdCard", "Verifying ID number: ", HttpContext.Connection.RemoteIpAddress.ToString(), Dns.GetHostName().ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception occured on api/customeraccounts/validateIdCard -> {ex.Message}", ex);
                return Ok(new ApiResponse<object>(ResponseCodeConstants.INTERNAL_EXCEPTION, ex.Message));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Make API call to smile
            var (response, code) = await _idVerificationService.VerifyId(request);

            if (code != HttpStatusCode.OK)
            {
                var res = new ApiResponse<SmileIDFullData>
                {
                    ResponseCode = "99",
                    ResponseDescription = "Unable to verify ID at the moment, please try again later.",
                    Data = null
                };
                return Ok(res);
            }

            // Handle when smile id is turned off
            if (response == null)
            {
                var fakeResponse = new ApiResponse<SmileIDFullData>
                {
                    ResponseCode = "00",
                    ResponseDescription = "Successful",
                    Data = null
                };

                return Ok(fakeResponse);
            }

            //Compare response to request
            var requestIsValid = ValidateIDRequestAndResponse(request, response);

            if (!requestIsValid)
                return Ok(new ApiResponse<object>("01", $"Name on ID card does not match the records in our database." +
                    $"Kindly input ID whose names match our records. For more enquiries," +
                    $"Please visit our branch or contact us on 0700 909 909 909 or CustomerCareNigeria@Stanbicibtc.com"));

            var result = new ApiResponse<SmileIDFullData>
            {
                ResponseCode = "00",
                ResponseDescription = "Successful",
                Data = response.FullData
            };

            return Ok(result);
        }

        [HttpPost("validateBVN")]
        [ProducesResponseType(200, Type = typeof(ApiResponse<SmileIDFullData>))]
        public async Task<IActionResult> ValidateBVN([FromBody] BvnValidationRequest request)
        {
            try
            {
                await _auditLogService.AuditLog(request.AccountNumber, "Debit Card", "ValidateBVN", "Verifying BVN number: ", HttpContext.Connection.RemoteIpAddress.ToString(), Dns.GetHostName().ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception occured on api/customeraccounts/validateBVN -> {ex.Message}", ex);
                return Ok(new ApiResponse<object>(ResponseCodeConstants.INTERNAL_EXCEPTION, ex.Message));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Make API call to validate BVN
            var (code, description, response) = await _customerAccountsService.DoBVNEnquiry(request.BvnId);
            if (code != "00")
                return Ok(new ApiResponse<object>(code, $"Failed to validate BVN details for {request.AccountNumber} at this time. {description}"));

            // Compare response to request
            var requestIsValid = ValidateBVNRequestAndResponse(request, response);

            if (!requestIsValid)
                return Ok(new ApiResponse<object>("01", $"Name on BVN - {request.BvnId} does not match the records in our database." +
                    $"Please update your primary account to match the names on your BVN"));

            var dateisValid = ValidateBVNDateOfBirth(request, response);

            if (!dateisValid)
                return Ok(new ApiResponse<object>("01", $"Date of birth - {DateTime.ParseExact(request.DateOfbirth, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToShortDateString()} entered does not match records on the bvn. " +
                    $"Please enter a correct Date of birth"));
            var result = new ApiResponse<CustomerAccountInfo>
            {
                ResponseCode = "00",
                ResponseDescription = description,
                Data = response
            };

            return Ok(result);
        }

        [HttpPost("getActiveCard")]
        [ProducesResponseType(200, Type = typeof(ApiResponse<SmileIDFullData>))]
        public async Task<IActionResult> GetActiveCard([FromBody] GetActiveCardRequest request)
        {
            try
            {
                await _auditLogService.AuditLog(request.AccountNumber, "Debit Card", "GetActiveCard", "Verifying BVN number: ", HttpContext.Connection.RemoteIpAddress.ToString(), Dns.GetHostName().ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception occured on api/customeraccounts/GetActiveCard -> {ex.Message}", ex);
                return Ok(new ApiResponse<object>(ResponseCodeConstants.INTERNAL_EXCEPTION, ex.Message));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Make API call to validate BVN
            var (code, description, response) = await _customerAccountsService.GetActiveCard(request.AccountNumber);
            if (code != "00")
                return Ok(new ApiResponse<object>("01", $"No active card is found for {request.AccountNumber} at this time."));

            var result = new ApiResponse<ActiveCardEnquiryInfo>
            {
                ResponseCode = "00",
                ResponseDescription = description,
                Data = response
            };

            return Ok(result);
        }

        [HttpPost("getCardType")]
        [ProducesResponseType(200, Type = typeof(ApiResponse<CardTypeResponse.CardPrograms>))]
        public async Task<IActionResult> GetCardType([FromBody] GetActiveCardRequest request)
        {
            try
            {
                await _auditLogService.AuditLog(request.AccountNumber, "Debit Card", "GetCardType", "Get Card type: ", HttpContext.Connection.RemoteIpAddress.ToString(), Dns.GetHostName().ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception occured on api/customeraccounts/GetCardType -> {ex.Message}", ex);
                return Ok(new ApiResponse<object>(ResponseCodeConstants.INTERNAL_EXCEPTION, ex.Message));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var (code, description, response) = await _customerAccountsService.GetCardType(request.AccountNumber);

                if (code != "00")
                    return Ok(new ApiResponse<object>("01", description));

                var result = new ApiResponse<CardTypeResponse.CardPrograms>
                {
                    ResponseCode = "00",
                    ResponseDescription = description,
                    Data = response
                };

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception occured on api/customeraccounts/GetCardType -> {e.Message}", e);

                return Ok(new ApiResponse<object>
                {
                    ResponseCode = "99",
                    ResponseDescription = "An Error Occurred",
                });
            }
        }

        [HttpGet("getCityState")]
        public async Task<IActionResult> GetCityState()
        {
            try
            {
                await _auditLogService.AuditLog("Nill - GET Request", "Debit Card", "GetCityState", "Retrieving City and States: ", HttpContext.Connection.RemoteIpAddress.ToString(), Dns.GetHostName().ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception occured on api/customeraccounts/getCityState -> {ex.Message}", ex);
                return Ok(new ApiResponse<object>(ResponseCodeConstants.INTERNAL_EXCEPTION, ex.Message));
            }

            try
            {
                var result = await _customerAccountsService.GetCityState();
                if (result.Count < 0)
                    return Ok(new ApiResponse<object>("99", $"Failed to retrieve City/State. "));

                var response = new ApiResponse<List<CityState>>
                {
                    ResponseCode = ResponseCodeConstants.SUCCESS,
                    ResponseDescription = "Successfully retrieved City/State",
                    Data = result
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception occured on api/customeraccounts/getCityState -> {ex.Message}", ex);
                return Ok(new ApiResponse<object>(ResponseCodeConstants.INTERNAL_EXCEPTION, ex.Message));
            }
        }

        [HttpGet("getBranches")]
        public async Task<IActionResult> GetBranches()
        {
            try
            {
                await _auditLogService.AuditLog("Nill - GET Request", "Debit Card", "GetBranches", "Retrieving City and States: ", HttpContext.Connection.RemoteIpAddress.ToString(), Dns.GetHostName().ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception occured on api/customeraccounts/getCityState -> {ex.Message}", ex);
                return Ok(new ApiResponse<object>(ResponseCodeConstants.INTERNAL_EXCEPTION, ex.Message));
            }

            try
            {
                var result = await _customerAccountsService.GetBranches();
                if (result.Count < 0)
                    return Ok(new ApiResponse<object>("99", $"Failed to retrieve City/State. "));

                var response = new ApiResponse<List<BankBranch>>
                {
                    ResponseCode = ResponseCodeConstants.SUCCESS,
                    ResponseDescription = "Successfully retrieved City/State",
                    Data = result
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception occured on api/customeraccounts/getCityState -> {ex.Message}", ex);
                return Ok(new ApiResponse<object>(ResponseCodeConstants.INTERNAL_EXCEPTION, ex.Message));
            }
        }

        private bool DoPhoneNumberValidation(string requestPhone, string accountPhone)
        {
            if (string.IsNullOrEmpty(requestPhone) || string.IsNullOrEmpty(accountPhone))
                return false;
            var sanitizedRequestPhone = SanitizePhoneForMatching(requestPhone);
            var sanitizedAccountPhone = SanitizePhoneForMatching(accountPhone);
            return sanitizedRequestPhone.Equals(sanitizedAccountPhone, StringComparison.OrdinalIgnoreCase);
        }

        private string SanitizePhoneForMatching(string text)
        {
            var output = new StringBuilder();
            int counter = 1;
            for (var i = text.Length - 1; i > 0 && counter <= 10; i--)
            {
                output.Append(text[i]);
                counter++;
            }
            return String.Join("", output.ToString().ToCharArray().Reverse());
        }

        private CustomerAccountInfo MapToCustomerInfo(AccountEnquiryInfo result, string accountSegment, CardTypes cardType)
        {
            return new CustomerAccountInfo
            {
                FirstName = result.FirstName,
                LastName = result.LastName,
                MiddleName = result.MiddleName,
                PhoneNumber = result.PhoneNumber,
                CifId = result.CustomerId,
                EmailAddress = result.EmailAddress,
                AccountName = result.AccountName,
                AccountSchemeCode = result.AccountSchemeCode,
                AccountSchemeType = result.AccountSchemeType,
                AccountSegment = accountSegment,
                BVN = result.Bvn,
                AccountStatus = result.AccountStatus,
                CustomerCreationDate = result.CustomerCreationDate,
                cardTypes = cardType != null ? cardType.types : new List<string>(),
                cardfees = cardType != null ? cardType.fees : new List<string>()
            };
        }

        private bool ValidateIDRequestAndResponse(IdVerificationRequest request, IdVerificationResponse apiResponse)
        {
            if (apiResponse.ResultCode == 1012)
            {
                try
                {
                    List<string> nameLists = new List<string> { apiResponse.FullData.firstName.ToLower(), apiResponse.FullData.lastName.ToLower() };

                    _logger.LogInformation(apiResponse.FullData.firstName.ToLower());
                    _logger.LogInformation(apiResponse.FullData.lastName.ToLower());

                    if (!nameLists.Contains(request.firstName.ToLower()) || !nameLists.Contains(request.lastName.ToLower()))
                        return false;
                    else return true;
                }
                catch (Exception ex) { }
                return true;
            }

            return false;
        }

        private bool ValidateBVNRequestAndResponse(BvnValidationRequest request, CustomerAccountInfo apiResponse)
        {
            List<string> nameLists = new List<string> { apiResponse.FirstName.ToLower(), apiResponse.LastName.ToLower() };

            if (!nameLists.Contains(request.LastName.ToLower()) || !nameLists.Contains(request.FirstName.ToLower()))
                return false;
            return true;
        }

        private bool ValidateBVNDateOfBirth(BvnValidationRequest request, CustomerAccountInfo apiResponse)
        {
            if (string.IsNullOrEmpty(apiResponse.DateOfBirth) || Convert.ToDateTime(request.DateOfbirth) != Convert.ToDateTime(apiResponse.DateOfBirth))
                return false;
            return true;
        }
    }
}