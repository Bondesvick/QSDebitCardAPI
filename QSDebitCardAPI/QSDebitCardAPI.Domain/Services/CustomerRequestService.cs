using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QSDataUpdateAPI.Domain.Services;
using QSDataUpdateAPI.Core.Interfaces.Services.Helpers.Redbox;
using QSDataUpdateAPI.Domain.Services.Helpers;
using QSDataUpdateAPI.Domain.Models.Requests.Redbox;
using QSDataUpdateAPI.Domain.Models.Requests;
using QSDataUpdateAPI.Data.Repositories;
using QSDataUpdateAPI.Core.Domain.Entities;
using QSDebitCardAPI.Domain.Services.RedboxServiceProxies.Interfaces;

namespace QSDataUpdateAPI.Domain.Services.RedboxProxies
{
    public class CustomerRequestService : ICustomerRequestService
    {
        private readonly IRepository<CustomerRequest, long> _customerRequestRepository;
        private readonly IRepository<DebitCardDetails, int> _accOpeningRepository;
        private readonly ICustomerRequestDataRepository _customerRequestDataRepository;
        private readonly IRedboxOtpServiceProxy _redboxOtpServiceProxy;
        private readonly IAppLogger _logger;
        private readonly IRedboxEmailService _emailServiceProxy;
        private readonly IRedboxSMSService _smsServiceProxy;
        private readonly IAppSettings _configSettings;
        private readonly IRedboxAccountServiceProxy _accountServiceProxy;

        public CustomerRequestService(IRepository<CustomerRequest, long> cusReqRepo, IAppLogger appLogger, IRedboxOtpServiceProxy otpServiceProxy
            , IRedboxEmailService emailService, IAppSettings settings, IRedboxSMSService smsService, IRedboxAccountServiceProxy accountServiceProxy,
            IRepository<DebitCardDetails, int> accOpeningRepository, ICustomerRequestDataRepository customerRequestDataRepository)
        {
            _customerRequestRepository = cusReqRepo;
            _logger = appLogger;
            _redboxOtpServiceProxy = otpServiceProxy;
            _emailServiceProxy = emailService;
            _smsServiceProxy = smsService;
            _accountServiceProxy = accountServiceProxy;
            _configSettings = settings;
            _accOpeningRepository = accOpeningRepository;
            _customerRequestDataRepository = customerRequestDataRepository;
        }

        public async Task<(bool status, string statusMessage, string result)> VerifyAndSaveAdditionalAccountOpeningRequest(DebitCardRequest request)
        {
            try
            {
                //var otpVerificationResponse = await _redboxOtpServiceProxy.VerifyOtp(new RedboxOtpVerificationRequest(request.OtpSourceReference, request.OtpReasonCode));
                var otpVerificationResponse = await _redboxOtpServiceProxy.VerifyOtpReqManager(request.OtpIdentifier, request.Otp, request.OtpSourceReference);
                if (otpVerificationResponse.responseCode != "00")
                    return (false, $"Otp verification failed. Check that {request.Otp} is correct and not expired and retry: {otpVerificationResponse.responseDescription}", otpVerificationResponse.responseDescription);

                //await _accountServiceProxy.DoStraightThroughSave(request);
                return await SaveAdditionalAccountOpeningRequest(request);
            }
            catch (Exception exception)
            {
                _logger.Error($"Error occured while saving debit card request", exception.Message, exception);
                return (false, "An error was encountered while saving customer request", null);
            }
        }

        public async Task<object> GetAccountOpeningRequest(int requestId)
        {
            try
            {
                return await _customerRequestRepository.GetItem(requestId);
            }
            catch (Exception exception)
            {
                _logger.Error("An Error occured while retrieving data update request", exception.Message, exception);
                throw;
            }
        }

        public async Task<IEnumerable<object>> GetAccountOpeningRequests()
        {
            try
            {
                return await _customerRequestRepository.GetItems();
            }
            catch (Exception exception)
            {
                _logger.Error($"An error occured while retrieving account requests: {exception.Message}", exception.Message, exception);
                throw;
            }
        }

        private async Task<(bool status, string statusMessage, string result)> SaveAdditionalAccountOpeningRequest(DebitCardRequest request)
        {
            try
            {
                #region STRAIGHT THROUGH TO FINACLE

                // Update phone and email on finacle

                var cifInfo = await _accountServiceProxy.DoAccountCIFEnquiry(request.AccountNumber);
                if (request.hotlistCode != null)
                {
                    var hotList = await _accountServiceProxy.DoHotlistCardEnquiry(request.AccountNumber, request.hotlistedCard, request.hotlistCode, request.expiryDate);

                    if (hotList.responseCode == "99" || hotList.responseCode == "999")
                    {
                        return (false, hotList.responseDescription, string.Empty);
                    }
                }
                var updatePhoneAndEmailResponse = await _accountServiceProxy.SubmitDebitCardEnquiry(request, cifInfo.result);
                if (updatePhoneAndEmailResponse.responseCode != "00")
                    return (false, updatePhoneAndEmailResponse.responseDescription, string.Empty);

                #endregion STRAIGHT THROUGH TO FINACLE

                var saveFinalSubmission = await SaveAndContinue(request, "RESOLVED");
                if (!saveFinalSubmission.status)
                    return (false, "Debit Card request submitted, but details could not be logged on the Database", string.Empty);

                // Send email
#pragma warning disable
                Task.Factory.StartNew(async () =>
                {
                    (string firstname, string phoneNo, string emailAddress) = await GetAccountDetailsAsync(request.AccountNumber);

                    if (!string.IsNullOrEmpty(phoneNo) || !string.IsNullOrEmpty(emailAddress))
                        await SendSubmissionNotificationMessageAsync(firstname, saveFinalSubmission.result, phoneNo, emailAddress, "Submit");
                });
#pragma warning restore

                //return (true, "Request saved successfully", saveFinalSubmission.result);
                return (true, "Request saved successfully", null);
            }
            catch (Exception exception)
            {
                var xx = exception;
                throw;
            }
        }

        private async Task SendSubmissionNotificationMessageAsync(string firstname, string ticketId, string phoneNo, string email, string task)
        {
            try
            {
                string message = "";

                switch (task)
                {
                    case ("SaveAndContinue"):
                        message = _configSettings.GetString("AppSettings:SaveAndContinueMessage");
                        break;

                    case ("Submit"):
                        message = _configSettings.GetString("AppSettings:SubmissionMessage");
                        break;
                }

                if (!string.IsNullOrEmpty(message))
                {
                    if (!string.IsNullOrEmpty(firstname))
                        message = message.Replace("#FirstName#", firstname).Replace("#TicketId#", ticketId);
                    else
                        message = message.Replace("#FirstName#", "customer").Replace("#TicketId#", ticketId);

                    if (!string.IsNullOrEmpty(email))
                        await SendNotificationEmailAsync(email, message);

                    if (!string.IsNullOrEmpty(phoneNo))
                        await SendNotificationSMSAsync(phoneNo, message);
                }
            }
            catch (Exception exception)
            {
                _logger.Error($"Error occured while sending notification message -> {exception.Message}", exception);
            }
        }

        private async Task SendNotificationEmailAsync(string emailAddress, string message)
        {
            var mailMessage = ComposeEmailMessage(emailAddress, message);
            var emailResponse = await _emailServiceProxy.SendEmailAsync(mailMessage);
            _logger.Info($"Email Response for {emailAddress} -> {emailResponse}");
        }

        private async Task SendNotificationSMSAsync(string phoneNumber, string message, string accountNumber = null)
        {
            var response = await _smsServiceProxy.SendSMSAsync(phoneNumber, message, accountNumber);
            _logger.Info($"SMS Response for {phoneNumber} -> {response}");
        }

        private RedboxEmailMessageModel ComposeEmailMessage(string email, string message)
        {
            var fromAddress = _configSettings.GetString("AppSettings:SenderEmail");
            var subject = _configSettings.GetString("AppSettings:EmailSubject");

            return new RedboxEmailMessageModel
            {
                FromAddress = fromAddress,
                ToAddress = email,
                Subject = subject,
                MailBody = message
            };
        }

        private async Task<(string firstname, string phoneNo, string emailAdd)> GetAccountDetailsAsync(string accountNo)
        {
            try
            {
                var accountInfo = await _accountServiceProxy.DoAccountCIFEnquiry(accountNo);
                if (accountInfo.responseCode == "00")
                    return (accountInfo.result?.FirstName, accountInfo.result?.PhoneNumber, accountInfo.result?.EmailAddress);

                _logger.Info($"Attempt to update customer onboarding request with Account name failed on CifEnquiry with account number {accountNo}");
            }
            catch (Exception exception)
            {
                _logger.Error($"Attempt to update customer onboarding request with Account name failed -> {exception}");
            }
            return default;
        }

        private IList<DebitCardDocument> BuildExtraAccountDocsFromPayload(DebitCardRequest request)
        {
            return request.Documents?.Select(doc => new DebitCardDocument { FileName = doc.Name, Title = doc.Title, ContentOrPath = doc.Base64Content, ContentType = GetDocumentContentType(doc.Name) }).ToList();
        }

        private string GetDocumentContentType(string fileName)
        {
            string contenttype = "";
            switch (fileName.Split('.')[1].ToLower())
            {
                case "doc":
                    contenttype = "application/vnd.ms-word";
                    break;

                case "docx":
                    contenttype = "application/vnd.ms-word";
                    break;

                case "pdf":
                    contenttype = "application/pdf";
                    break;

                case "jpg":
                    contenttype = "image/jpeg";
                    break;

                case "svg":
                    contenttype = "image/svg+xml";
                    break;

                case "jpeg":
                    contenttype = "image/jpeg";
                    break;

                case "png":
                    contenttype = "image/png";
                    break;

                case "gif":
                    contenttype = "image/gif";
                    break;
            }
            return contenttype;
        }

        private CustomerRequest BuildCustomerRequestEntityFromPayload(DebitCardRequest request, string status)
        {
            return new CustomerRequest
            {
                CreatedDate = DateTime.Now,
                RequestType = "Debit Card Request",
                CustomerAuthType = GetAuthType(request.AuthType),
                Status = status,
                TreatedByUnit = "ACCOUNT ORIGINATION",
                AccountName = request.AccountName,
                AccountNumber = request.AccountNumber,
                Bvn = request.BVN
            };
        }

        private string GetAuthType(string authType)
        {
            if (authType.ToLower().Equals("signature"))
                return "OTP";
            if (authType.ToLower().Equals("debit-card"))
                return "WITH_DEBIT_CARD";
            return authType.ToUpper();
        }

        public async Task<(bool status, string statusMessage, string result)> SaveAndContinue(DebitCardRequest request, string status)
        {
            try
            {
                CustomerRequest customerRequest = BuildCustomerRequestEntityFromPayload(request, status);

                customerRequest.DebitCardDetails = BuildDebitCardDetails(request);

                CustomerRequest result = new CustomerRequest();

                //Check if caseId is available and handle cases
                if (String.IsNullOrEmpty(request.CaseId))
                {
                    var saveResult = await _customerRequestRepository.AddItem(customerRequest);
                    if (saveResult == null || saveResult.Id < 1)
                        return (false, "Could not save debit card request at this time. please retry after some time", string.Empty);

                    saveResult.TranId = GenerateTranId(saveResult.Id);
                    saveResult.DebitCardDetails.FirstOrDefault().CaseId = saveResult.TranId;
                    await _customerRequestDataRepository.UpdateCustomerRequest(saveResult);

                    // Send notification for new case id
                    await Task.Factory.StartNew(async () =>
                     {
                         (string firstname, string phoneNo, string emailAddress) = await GetAccountDetailsAsync(request.AccountNumber);

                         if (!string.IsNullOrEmpty(phoneNo) || !string.IsNullOrEmpty(emailAddress))
                             await SendSubmissionNotificationMessageAsync(firstname, saveResult.TranId, phoneNo, emailAddress, "SaveAndContinue");
                     });

                    result = saveResult;
                }
                else
                {
                    //retrieve saved session
                    var id = Convert.ToInt32(ExtractIdFromTranId(request.CaseId));
                    if (id == 0)
                        return (false, "Invalid Case ID. Please confirm that the case ID is correct", null);

                    var existingSession = await _customerRequestDataRepository.GetCustomerRequestById(id);

                    if (existingSession == null || existingSession.Id < 1)
                        return (false, "Could not save debit card request at this time. please retry after some time", string.Empty);

                    // Set IDs
                    customerRequest.Id = existingSession.Id;
                    customerRequest.DebitCardDetails.FirstOrDefault().Id = existingSession.DebitCardDetails.FirstOrDefault().Id;
                    customerRequest.TranId = customerRequest.DebitCardDetails.FirstOrDefault().CaseId;

                    //update with new record
                    var updateResult = await _customerRequestDataRepository.UpdateCustomerRequest(customerRequest);

                    result = updateResult;
                }

                return (true, "Request saved successfully", result.TranId);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error occured while saving data update progress", ex.Message, ex);
                return (false, "Error occured while saving data update progress", ex.Message);
            }
        }

        private string GenerateTranId(long id)
        {
            return $"DB-CD-{id}";
        }

        private List<DebitCardDetails> BuildDebitCardDetails(DebitCardRequest request)
        {
            var result = new List<DebitCardDetails>
            {
                new DebitCardDetails {
                    Documents = BuildExtraAccountDocsFromPayload(request),
                    CaseId = request.CaseId,
                    CurrentStep = request.CurrentStep,
                    Submitted = request.Submitted,
                    IAcceptTermsAndCondition = request.IAcceptTermsAndCondition,
                    DateOfAcceptingTAndC = DateTime.Now,
                    DateOfBirth = request.DateOfBirth,
                    AccountStatus = request.AccountStatus,
                    AccountToDebit = request.AccountToDebit,
                    AuthType = request.AuthType,
                    hotlistedCard = request.hotlistedCard,
                    hotlistCode = request.hotlistCode,
                    Branch = request.Branch,
                    City = _configSettings.GetString("AppSettings:City"),
                    Title = _configSettings.GetString("AppSettings:Title"),
                    Gender = _configSettings.GetString("AppSettings:Gender"),
                    MaritalStatus = _configSettings.GetString("AppSettings:MaritalStatus"),
                    BVN = request.BVN,
                    NameOnCard = request.NameOnCard,
                    PhoneNumber = request.PhoneNumber,
                    RequestType = request.RequestType,
                    CardType  = _configSettings.GetString("AppSettings:CardType")
                }
            };

            return result;
        }

        public async Task<(bool status, string statusMessage, DebitCardDetails result)> VerifyCaseId(string caseId)
        {
            if (!caseId.Contains('-'))
                return (false, "Invalid Case ID. Please confirm that the case ID is correct", null);

            var id = Convert.ToInt32(ExtractIdFromTranId(caseId));
            if (id == 0)
                return (false, "Invalid Case ID. Please confirm that the case ID is correct", null);

            try
            {
                var customerReq = await _customerRequestDataRepository.GetCustomerRequestById(id);
                if (customerReq == null)
                    return (false, "Could not retrieve case id at this time. please retry after some time", null);

                var result = customerReq.DebitCardDetails.FirstOrDefault();
                if (result == null || result.Id < 1)
                    return (false, "Could not retrieve case id at this time. please retry after some time", null);
                return (true, "Data update progress retrieved successfully", result);
            }
            catch (Exception exception)
            {
                _logger.Error("An Error occured while retrieving debit card progress", exception.Message, exception);
                throw;
            }
        }

        private long ExtractIdFromTranId(string tranId)
        {
            var stringId = "";
            try
            {
                stringId = tranId.Split('-')[1];
            }
            catch (Exception)
            {
                return 0;
            }

            return long.Parse(stringId);
        }
    }
}