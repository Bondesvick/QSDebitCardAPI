using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using QSDataUpdateAPI.Core.Domain.Entities;
using QSDataUpdateAPI.Core.Interfaces.Services.Helpers.Redbox;
using QSDataUpdateAPI.Domain;
using QSDataUpdateAPI.Domain.Models.Requests;
using QSDataUpdateAPI.Domain.Models.Response;
using QSDataUpdateAPI.Domain.Models.Response.Redbox;
using QSDataUpdateAPI.Domain.Services.Helpers;
using QSDebitCardAPI.Data.Data.Entities;
using QSDebitCardAPI.Domain.Models.Response.Redbox;
using QSDebitCardAPI.Domain.Services.Helpers;
using QSDebitCardAPI.Domain.Services.RedboxServiceProxies.Interfaces;

namespace QSDebitCardAPI.Domain.Services.RedboxServiceProxies
{
    public class AccountServiceProxy : IRedboxAccountServiceProxy
    {
        private readonly IAppLogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IRedboxRequestManagerProxy _requestManagerProxy;
        private readonly ISoapRequestHelper _soapRequestHelper;
        private readonly IAppSettings _configSettings;

        public AccountServiceProxy(
            IConfiguration configuration,
            IAppLogger logger,
            IRedboxRequestManagerProxy requestManagerProxy,
            ISoapRequestHelper soapRequestHelper,
            IAppSettings settings)
        {
            _logger = logger;
            _requestManagerProxy = requestManagerProxy;
            _soapRequestHelper = soapRequestHelper;
            _configuration = configuration;
            _configSettings = settings;
        }

        public async Task<(string responseCode, string responseDescription, CustomerAccountInfo result)> GetCustomerAccountDetails(string accountNumber, string phoneNumber)
        {
            try
            {
                var fetchCustomerAccountInfoPayload = FormFetchCustomerRequestPayload(phoneNumber, accountNumber);
                var response = await _requestManagerProxy.Post<CustomerAccountInfo>(fetchCustomerAccountInfoPayload);
                if (response.ResponseCode == "00" || response.ResponseCode == "000")
                {
                    return ("00", response.ResponseDescription, response.Model);
                }
                return (response.ResponseCode, response.ResponseDescription, null);
            }
            catch (Exception exception)
            {
                _logger.Error("Exception occured while fetching customer profile", ex: exception);
                throw;
            }
        }

        public async Task<(string responseCode, string responseDescription, string segment)> GetAccountSegment(string accountNumber)
        {
            try
            {
                var fetchAccountSegment = FormGetCustomerAccountSegmentRequestPayload(accountNumber);
                var response = await _requestManagerProxy.Post<string>(fetchAccountSegment);
                if (response.ResponseCode == "00" || response.ResponseCode == "000" || response.ResponseCode == "202")
                {
                    var accountSegment = Util.GetXmlTagValue(response.Detail, "segmentName");
                    return ("00", response.ResponseDescription, accountSegment);
                }
                return (response.ResponseCode, response.ResponseDescription, null);
            }
            catch (Exception exception)
            {
                _logger.Error("Exception occured while fetching customer profile", ex: exception);
                throw;
            }
        }

        public async Task<(string responseCode, string responseDescription, CustomerAccountInfo result)> DoCorporateAccountEnquiry(string accountNumber)
        {
            try
            {
                var fetchCustomerAccountInfoPayload = FormCorpAccountEnquiryRequestPayload(accountNumber);
                var response = await _requestManagerProxy.Post<string>(fetchCustomerAccountInfoPayload);
                if (response.ResponseCode == "00" || response.ResponseCode == "000" || response.ResponseCode == "202")
                {
                    return ("00", response.ResponseDescription, BuildAccountInfoFromResponse(response.Detail));
                }
                return (response.ResponseCode, response.ResponseDescription, null);
            }
            catch (Exception exception)
            {
                _logger.Error("Exception occured while doing corporate account name enquiry", ex: exception);
                throw;
            }
        }

        public async Task<(string responseCode, string responseDescription, AccountEnquiryInfo result)> DoAccountCIFEnquiry(string accountNumber)
        {
            try
            {
                var fetchCustomerAccountInfoPayload = FormAccountCifEnquiryRequestPayload(accountNumber);
                var response = await _requestManagerProxy.Post<string>(fetchCustomerAccountInfoPayload);
                if (response.ResponseCode == "00" || response.ResponseCode == "000" || response.ResponseCode == "202")
                {
                    return ("00", response.ResponseDescription, BuildAccountEnquiryInfoFromResponse(response.Detail));
                }
                return (response.ResponseCode, response.ResponseDescription, null);
            }
            catch (Exception exception)
            {
                _logger.Error("Exception occured while doing corporate account name enquiry", ex: exception);
                throw;
            }
        }

        public async Task<(string responseCode, string responseDescription)> DoHotlistCardEnquiry(string accountNumber, string maskPan, string hotlistCode, string exp)
        {
            try
            {
                var fetchCustomerAccountInfoPayload = BlockCardEnquiryRequestPayload(accountNumber, maskPan, hotlistCode, exp);
                var response = await _requestManagerProxy.Post<string>(fetchCustomerAccountInfoPayload);
                if (response.ResponseCode == "00" || response.ResponseCode == "000" || response.ResponseCode == "202")
                {
                    return ("00", response.ResponseDescription);
                }
                return (response.ResponseCode, response.ResponseDescription);
            }
            catch (Exception exception)
            {
                _logger.Error("Exception occured while doing corporate account name enquiry", ex: exception);
                throw;
            }
        }

        public async Task<(string responseCode, string responseDescription, CardTypes result)> DebitCardTypes(string accountNumber)
        {
            try
            {
                var fetchDebitCardTypePayload = CardTypeRequestPayload(accountNumber);
                var response = await _requestManagerProxy.Post<string>(fetchDebitCardTypePayload);
                if (response.ResponseCode == "00" || response.ResponseCode == "000" || response.ResponseCode == "202")
                {
                    return ("00", response.ResponseDescription, BuildDebitCardTypeResponse(response.Detail));
                }
                return (response.ResponseCode, response.ResponseDescription, null);
            }
            catch (Exception exception)
            {
                _logger.Error("Exception occured while doing corporate account name enquiry", ex: exception);
                throw;
            }
        }

        public async Task<(string responseCode, string responseDescription)> SubmitDebitCardEnquiry(DebitCardRequest debitCardDetails, AccountEnquiryInfo cifInfo)
        {
            try
            {
                var fetchDebitCardInfoPayload = SaveDebitCardEnquiryRequestPayload(debitCardDetails, cifInfo);
                var response = await _requestManagerProxy.Post<string>(fetchDebitCardInfoPayload);
                if (response.ResponseCode == "00" || response.ResponseCode == "000" || response.ResponseCode == "202")
                {
                    return ("00", response.ResponseDescription);
                }
                return (response.ResponseCode, response.ResponseDescription);
            }
            catch (Exception exception)
            {
                _logger.Error("Exception occured while doing corporate account name enquiry", ex: exception);
                throw;
            }
        }

        public async Task<(string responseCode, string responseDescription, CustomerAccountInfo result)> DoBVNEnquiry(string bvnId)
        {
            try
            {
                var fetchBVNEnquiryPayload = BuildBVNValidationRequestPayload(bvnId);
                var response = await _requestManagerProxy.Post<string>(fetchBVNEnquiryPayload);
                if (response.ResponseCode == "00" || response.ResponseCode == "000" || response.ResponseCode == "202")
                {
                    return ("00", response.ResponseDescription, BuildAccountInfoFromBVNEnquiryResponse(response.Detail));
                }
                return (response.ResponseCode, response.ResponseDescription, null);
            }
            catch (Exception exception)
            {
                _logger.Error("Exception occured while doing BVN enquiry", ex: exception);
                throw;
            }
        }

        public async Task<(string responseCode, string responseDescription, ActiveCardEnquiryInfo result)> GetActiveCard(string accountNumber)
        {
            try
            {
                var fetchActiveCardPayload = BuildGetActiveCardPayload(accountNumber);
                var response = await _requestManagerProxy.Post<string>(fetchActiveCardPayload);
                if (response.ResponseCode == "00" || response.ResponseCode == "000" || response.ResponseCode == "202")
                {
                    return ("00", response.ResponseDescription, BuildActiveCardEnquiryResponse(response.Detail));
                }
                return (response.ResponseCode, response.ResponseDescription, null);
            }
            catch (Exception exception)
            {
                _logger.Error("Exception occured while doing BVN enquiry", ex: exception);
                throw;
            }
        }

        public async Task<(string responseCode, string responseDescription, CardTypeResponse.CardPrograms result)> GetCardType(string accountNumber)
        {
            try
            {
                var fetchActiveCardPayload = BuildGetCardTypePayload(accountNumber);
                var endpoint = _configuration["AppSettings:RedboxReqMngr"];

                // var reqResponse = await _soapRequestHelper.SoapCall(fetchActiveCardPayload, "\"treat\"", endpoint);

                var response = await _requestManagerProxy.Post<string>(fetchActiveCardPayload);
                if (response.ResponseCode == "00" || response.ResponseCode == "000" || response.ResponseCode == "202")
                {
                    return ("00", response.ResponseDescription, BuildCardTypeEnquiryResponse(response.Detail));
                }
                return (response.ResponseCode, response.ResponseDescription, null);
            }
            catch (Exception exception)
            {
                _logger.Error($"Exception occurred while getting card type: {exception.Message}", ex: exception);
                throw;
            }
        }

        public async Task<List<CityState>> GetCityState()
        {
            string sql = "Select * FROM [QuickService].[dbo].[CITY_STATE] ORDER BY region asc";

            using (var connection = new SqlConnection(_configuration.GetConnectionString("QuickServiceDbConn")))
            {
                var cityState = connection.QueryAsync<CityState>(sql).Result.ToList();

                return cityState;
            }
        }

        public async Task<List<BankBranch>> GetBranches()
        {
            string sql = "Select * FROM [QuickService].[dbo].[BANK_BRANCHES] ORDER BY branch asc";

            using (var connection = new SqlConnection(_configuration.GetConnectionString("QuickServiceDbConn")))
            {
                var bankBranch = connection.QueryAsync<BankBranch>(sql).Result.ToList();

                return bankBranch;
            }
        }

        #region private_helpers

        private string FormFetchCustomerRequestPayload(string phoneNumber, string accountNumber)
        {
            string payload = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:soap=""http://soap.request.manager.redbox.stanbic.com/"">
							   <soapenv:Header/>
								<soapenv:Body>
									<soap:request>
										<reqTranId>{Util.TimeStampCode()}</reqTranId>
										<channel>USSD</channel>
										<type>FETCH_CUSTOMER</type>
										<customerId>{accountNumber}</customerId>
										<customerIdType>ACCOUNT_NUMBER</customerIdType>
										<submissionTime>{DateTime.Now}</submissionTime>
										<body>
											<otherRequestDetails>
												<passCode />
												<passId>{phoneNumber}</passId>
												<passIdType>PHONE_NUMBER</passIdType>
												<passCodeType>01</passCodeType>
											</otherRequestDetails>
										  </body>
									 </soap:request>
							   </soapenv:Body>
							  </soapenv:Envelope>";
            return payload;
        }

        private string FormGetCustomerAccountSegmentRequestPayload(string accountNumber)
        {
            string payload = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:soap=""http://soap.request.manager.redbox.stanbic.com/"">
							   <soapenv:Header/>
								<soapenv:Body>
									<soap:request>
										<reqTranId>{Util.TimeStampCode()}</reqTranId>
										<channel>INTERNET_BANKING</channel>
										<type>GET_ACCOUNT_SEGMENT</type>
										<customerId>{accountNumber}</customerId>
										<customerIdType>ACCOUNT_NUMBER</customerIdType>
										<submissionTime>{DateTime.Now:f}</submissionTime>
										<body></body>
									 </soap:request>
							   </soapenv:Body>
							  </soapenv:Envelope>";
            return payload;
        }

        private string FormCorpAccountEnquiryRequestPayload(string accountNumber)
        {
            var payload = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:soap=""http://soap.request.manager.redbox.stanbic.com/"">
   <soapenv:Header/>
   <soapenv:Body>
	  <soap:request>
		 <reqTranId>{Util.TimeStampCode()}</reqTranId>
		 <channel>BPM</channel>
		 <type>ACCOUNT_ENQUIRY</type>
		 <submissionTime>{DateTime.Now.ToString("o")}</submissionTime>
		 <body><![CDATA[<otherRequestDetails>
		 <cifId></cifId>
		<accountNumber>{accountNumber}</accountNumber>
 <moduleTranReferenceId>34343434343</moduleTranReferenceId>
	  </otherRequestDetails>]]></body>
	  </soap:request>
   </soapenv:Body>
</soapenv:Envelope>";
            return payload;
        }

        private CustomerAccountInfo BuildAccountInfoFromResponse(string detail)
        {
            _logger.Info(detail);
            return new CustomerAccountInfo
            {
                BVN = Util.GetTagValue(detail, "AccountBvn"),
                FirstName = Util.GetTagValue(detail, "CustomerFirstName"),
                LastName = Util.GetTagValue(detail, "CustomerLastName"),
                EnrollmentBank = "221",
                EnrollmentBranch = Util.GetTagValue(detail, "AccountBranchName"),
                CifId = Util.GetTagValue(detail, "CustomerId")
            };
        }

        private AccountEnquiryInfo BuildAccountEnquiryInfoFromResponse(string detail)
        {
            AccountEnquiryInfo accountEnquiryInfo = new AccountEnquiryInfo
            {
                FirstName = Util.GetFirstTagValue(detail, "FirstName", ignoreCase: false),
                LastName = Util.GetFirstTagValue(detail, "LastName", ignoreCase: false),
                EmailAddress = Util.GetFirstTagValue(detail, "Email", ignoreCase: false),
                PhoneNumber = Util.GetTagValue(detail, "PhoneNumber1"),
                PhoneNumber1 = Util.GetTagValue(detail, "PhoneNumber2"),
                AccountName = Util.GetTagValue(detail, "AccountName"),
                AccountSchemeCode = Util.GetTagValue(detail, "AccountSchemeCode"),
                AccountSchemeType = Util.GetTagValue(detail, "AccountSchemeType"),
                AccountCurrencyCode = Util.GetTagValue(detail, "AccountCurrencyCode"),
                Bvn = Util.GetTagValue(detail, "Bvn").Length <= 11 ? Util.GetTagValue(detail, "Bvn") : Util.GetTagValue(detail, "Bvn").Substring(0, 11),
                CustomerCreationDate = Util.GetTagValue(detail, "AccountOpenDate"),
                AccountStatus = Util.GetTagValue(detail, "AccountStatus"),
                CustomerId = Util.GetTagValue(detail, "CustId"),
                AvailableBalance = Util.GetTagValue(detail, "AvailableBalance"),
                City = Util.GetFirstTagValue(detail, "cityCode"),
                Gender = Util.GetFirstTagValue(detail, "Gender"),
                MaritalStatus = Util.GetFirstTagValue(detail, "MaritalStatus"),
                Title = Util.GetFirstTagValue(detail, "Salutation")
            };

            var firstTag = Util.GetFirstTagValue(detail, "PhoneEmailType");
            var secondTag = Util.GetSecondTagValue(detail, "PhoneEmailType");
            if (firstTag == "HOMEEML")
            {
                accountEnquiryInfo.PhoneEmailIdEmail = Util.GetFirstTagValue(detail, "PhoneEmailId");
                accountEnquiryInfo.PhoneEmailIdEmailType = "HOMEEML";
            }
            else if (firstTag == "CELLPH")
            {
                accountEnquiryInfo.PhoneEmailIdPhone = Util.GetFirstTagValue(detail, "PhoneEmailId");
                accountEnquiryInfo.PhoneEmailIdPhoneType = "CELLPH";
            }

            if (secondTag == "HOMEEML")
            {
                accountEnquiryInfo.PhoneEmailIdEmail = Util.GetSecondTagValue(detail, "PhoneEmailId");
                accountEnquiryInfo.PhoneEmailIdEmailType = "HOMEEML";
            }
            else if (secondTag == "CELLPH")
            {
                accountEnquiryInfo.PhoneEmailIdPhone = Util.GetSecondTagValue(detail, "PhoneEmailId");
                accountEnquiryInfo.PhoneEmailIdPhoneType = "CELLPH";
            }
            return accountEnquiryInfo;
        }

        private CardTypes BuildDebitCardTypeResponse(string detail)
        {
            CardTypes cardTypes = new CardTypes
            {
                types = Util.GetAllMarchedTagValue(detail, "cardType"),
                fees = Util.GetAllMarchedTagValue(detail, "fee")
            };
            return cardTypes;
        }

        private TinEnquiryInfo BuildTinEnquiryInfoFromResponse(string detail)
        {
            return new TinEnquiryInfo
            {
                AccountName = Util.GetTagValue(detail, "taxPayername"),
            };
        }

        private CustomerAccountInfo BuildAccountInfoFromBVNEnquiryResponse(string detail)
        {
            return new CustomerAccountInfo
            {
                BVN = Util.GetTagValue(detail, "bvn"),
                FirstName = Util.GetTagValue(detail, "firstName"),
                LastName = Util.GetTagValue(detail, "lastName"),
                EnrollmentBank = "221",
                EnrollmentBranch = Util.GetTagValue(detail, "enrollmentBranch"),
                CifId = Util.GetTagValue(detail, "cifId"),
                DateOfBirth = Util.GetTagValue(detail, "dateOfBirth"),
                MaskedPhoneNumber = Util.GetTagValue(detail, "maskedPhoneNumber")
            };
        }

        private ActiveCardEnquiryInfo BuildActiveCardEnquiryResponse(string detail)
        {
            return new ActiveCardEnquiryInfo
            {
                maskedPAN = Util.GetTagValue(detail, "maskedPAN"),
                expiryDate = Util.GetTagValue(detail, "expiryDate")
            };
        }

        private CardTypeResponse.CardPrograms BuildCardTypeEnquiryResponse(string detail)
        {
            var cardPrograms = Util.GetTagValue(detail, "cardPrograms");

            //var serializer = new XmlSerializer(typeof(CardTypeResponse.Detail));
            //using var reader = new StringReader(cardPrograms);
            //var test = (CardTypeResponse.CardPrograms)serializer.Deserialize(reader);

            var value = Util.DeserializeXML<CardTypeResponse.CardPrograms>(detail);

            return value;
        }

        private string FormAccountCifEnquiryRequestPayload(string accountNumber)
        {
            var reqTranId = Util.TimeStampCode();
            var payload = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:soap=""http://soap.request.manager.redbox.stanbic.com/"">
							   <soapenv:Header/>
							   <soapenv:Body>
								  <soap:request>
									 <reqTranId>{reqTranId}</reqTranId>
									 <channel>INTERNET_BANKING</channel>
									 <type>CIF_ENQUIRY</type>
									 <submissionTime>{DateTime.Now.ToString("o")}</submissionTime>
									 <body><![CDATA[<otherRequestDetails>
									 <cifId></cifId>
									 <cifType></cifType>
									<accountNumber>{accountNumber}</accountNumber>
									<moduleTranReferenceId>{reqTranId}</moduleTranReferenceId>
								  </otherRequestDetails>]]></body>
								  </soap:request>
							   </soapenv:Body>
							</soapenv:Envelope>";
            return payload;
        }

        private string BlockCardEnquiryRequestPayload(string accountNumber, string maskPan, string hotlistCode, string exp)
        {
            var reqTranId = Util.TimeStampCode();

            var payload = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:soap=""http://soap.request.manager.redbox.stanbic.com/"">
		 <soapenv:Header/>
		  <soapenv:Body>
			   <ns2:request xmlns:ns2=""http://soap.request.manager.redbox.stanbic.com/"">
					<reqTranId>{reqTranId}</reqTranId>
							   <channel>MOBILE_APP</channel>
							   <type>BLOCK_CARD</type>
							   <customerId>{accountNumber}</customerId>
							   <customerIdType>ACCOUNT_NUMBER</customerIdType>
							   <body><![CDATA[<otherRequestDetails><maskedPAN>{maskPan}</maskedPAN><expiryDate>{exp}</expiryDate><hotlistCode>{hotlistCode}</hotlistCode>
								<passCode>...</passCode></otherRequestDetails>]]></body>
							   <submissionTime>{DateTime.Now.ToString("o")}</submissionTime>
									 </ns2:request>
								  </soapenv:Body>
							   </soapenv:Envelope>";

            return payload;
        }

        private string SaveDebitCardEnquiryRequestPayload(DebitCardRequest debitCardDetails, AccountEnquiryInfo cifInfo)
        {
            var gender = _configSettings.GetString("AppSettings:Gender");
            var title = _configSettings.GetString("AppSettings:Title");
            var maritalStatus = _configSettings.GetString("AppSettings:MaritalStatus");
            var city = _configSettings.GetString("AppSettings:City");
            var cardType = _configSettings.GetString("AppSettings:CardType");

            var reqTranId = Util.TimeStampCode();
            var payload = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:soap=""http://soap.request.manager.redbox.stanbic.com/"">
							   <soapenv:Header/>
							   <soapenv:Body>
			<soap:request xmlns:soap=""http://soap.request.manager.redbox.stanbic.com/"" xmlns:ns2=""http://soap.request.manager.redbox.stanbic.com/"">
			<channel>MOBILE_APP</channel>
			<type>REQ_CARD</type>
			<customerId>{debitCardDetails.AccountNumber}</customerId>
			<customerIdType>ACCOUNT_NUMBER</customerIdType>
			<submissionTime>{DateTime.Now}</submissionTime>
			<reqTranId>{reqTranId}</reqTranId><body><![CDATA[<otherRequestDetails>
			<cardType>{cardType}</cardType>
			<collectionSol>{debitCardDetails.Branch}</collectionSol>
			<initiationSol>{debitCardDetails.Branch}</initiationSol>
			<gender>{gender}</gender>
			<titleID>{title}</titleID>
			<maritalStatusID>{maritalStatus}</maritalStatusID>
			<city>{city}</city>
			<nameOnCard>{debitCardDetails.NameOnCard}</nameOnCard>
			<debitAccountNo>{debitCardDetails.AccountNumber}</debitAccountNo>
		</otherRequestDetails>]]></body>
			</soap:request>
	   </soapenv:Body>
							</soapenv:Envelope>";
            return payload;
        }

        private string CardTypeRequestPayload(string accountNumber)
        {
            var reqTranId = Util.TimeStampCode();

            var payload = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:soap=""http://soap.request.manager.redbox.stanbic.com/"">
	   <soapenv:Header/>
			<soapenv:Body>
			  <ns2:request xmlns:ns2=""http://soap.request.manager.redbox.stanbic.com/"" xmlns:soap=""http://soap.request.manager.redbox.stanbic.com/"">
				  <channel>MOBILE_APP</channel>
				  <type>GET_CARD_TYPE</type>
				  <customerId>{accountNumber}</customerId>
				  <customerIdType>ACCOUNT_NUMBER</customerIdType>
				  <reqTranId>{reqTranId}</reqTranId>
				  <body><![CDATA[<otherRequestDetails></otherRequestDetails>]]></body>
				  <submissionTime>{DateTime.Now}</submissionTime>
				</ns2:request>
			</soapenv:Body>
	   </soapenv:Envelope>";
            //         var payload = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:soap=""http://soap.request.manager.redbox.stanbic.com/"">
            //    < soapenv:Header />
            //                             < soapenv:Body>
            //<ns2:requestxmlns:ns2=""http://soap.request.manager.redbox.stanbic.com/""xmlns:soap=""http://soap.request.manager.redbox.stanbic.com/"">
            //             <channel>MOBILE_APP</channel>
            //             <type>GET_CARD_TYPE</type>
            //             <customerId>{accountNumber}</customerId>
            //             <customerIdType>ACCOUNT_NUMBER</customerIdType>
            //             <reqTranId>{reqTranId}</reqTranId>
            //             <body><![CDATA[<otherRequestDetails></otherRequestDetails>]]></body>
            //             <submissionTime>{DateTime.Now}</submissionTime>
            //                </ns2:request>
            //               </soapenv:Body>
            //                                     </soapenv:Envelope>";
            return payload;
        }

        public async Task<(bool, LoanRepaymentDetailDTO, string)> AccountSchemeEnquiry(string accountNumber)
        {
            try
            {
                var payload = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:soap=""http://soap.request.manager.redbox.stanbic.com/"">
							 <soapenv:Header/>
								<soapenv:Body>
									<soap:request>
										<reqTranId>{GeneralHelpers.GenerateRequestTranID()}</reqTranId>
										<channel>BPM</channel>
										<type>LOAN_ACCOUNT_ENQUIRY</type>
										<body><![CDATA[<otherRequestDetails>
										<accountNumber>{accountNumber}</accountNumber>
										<moduleTranReferenceId >{GeneralHelpers.GenerateRequestTranID()}</moduleTranReferenceId>
										</otherRequestDetails>]]></body>
										<submissionTime>2018-03-06T17:12:39.669+01:00</submissionTime>
									</soap:request>
								</soapenv:Body>
							</soapenv:Envelope>";

                var response = await _requestManagerProxy.Post<string>(payload);

                if (response.ResponseCode == "00" || response.ResponseCode == "000" || response.ResponseCode == "202")
                {
                    return (true, BuildAccountSchemeResponseDetail(response.Detail), response.ResponseDescription);
                }

                _logger.Error("Failure Response: " + response, "GetCustomerInfoByAccountNumberAsync");

                return (false, null, response.ResponseDescription);
            }
            catch (Exception ex)
            {
                _logger.Error("Exception: " + ex.Message, "fetch Loan enquiry");
            }

            return default;
        }

        private LoanRepaymentDetailDTO BuildAccountSchemeResponseDetail(string detail)
        {
            return new LoanRepaymentDetailDTO
            {
                AccountName = Util.GetTagValue(detail, "AccountName"),
                AccountNumber = Util.GetTagValue(detail, "AccountNumber"),
                AvailableBalance = Util.GetTagValue(detail, "AvailableBalance"),
                LoanAmountValue = Util.GetTagValue(detail, "LoanAmountValue"),
                OutstandingBalance = Util.GetTagValue(detail, "OutstandingBalance"),
                AccountSchemeType = Util.GetTagValue(detail, "AccountSchemeType"),
                AccountSchemeCode = Util.GetTagValue(detail, "AccountSchemeCode"),
                AccountType = Util.GetTagValue(detail, "AccountType")
            };
        }

        private string FormCustomerTinEnquiryRequestPayload(string tinNumber)
        {
            var reqTranId = Util.TimeStampCode();
            var payload = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:soap=""http://soap.request.manager.redbox.stanbic.com/"">
							  <soapenv:Header/>
							   <soapenv:Body>
								  <soap:request>
									 <reqTranId>{reqTranId}</reqTranId>
									 <channel>BPM|AOUSER</channel>
									 <type>VALIDATE_TIN</type>
									 <customerId>{tinNumber}</customerId>
									 <customerIdType>TIN </customerIdType>
									 <body/>
									 <submissionTime>{DateTime.Now.ToString("o")}</submissionTime>
								  </soap:request>
							   </soapenv:Body>
							</soapenv:Envelope>";
            return payload;
        }

        private string BuildBVNValidationRequestPayload(string bvnId)
        {
            var payload = $@"<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
								<soap:Body>

									<request xmlns=""http://soap.request.manager.redbox.stanbic.com/""
										xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
										xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
										<reqTranId xmlns="""">154673685</reqTranId>
										<channel xmlns="""">MOBILE_APP</channel>
										<type xmlns="""">FETCH_CUSTOMER</type>
										<customerId xmlns="""">{bvnId}</customerId>
										<customerIdType xmlns="""">BVN</customerIdType>
										<body xmlns=""""/>
										<submissionTime>{DateTime.Now}</submissionTime>
									</request>

								</soap:Body>
							</soap:Envelope>";
            return payload;
        }

        private string BuildGetActiveCardPayload(string accountNumber)
        {
            var payload = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:soap=""http://soap.request.manager.redbox.stanbic.com/"">
	<soapenv:Header/>
	 <soapenv:Body>
		  <ns2:request xmlns:ns2=""http://soap.request.manager.redbox.stanbic.com/"">
			   <reqTranId>{Util.TimeStampCode()}</reqTranId>
						  <channel>MOBILE_APP</channel>
						  <type>GET_ACTIVE_CARDS</type>
						  <customerId>{accountNumber}</customerId>
						  <customerIdType>ACCOUNT_NUMBER</customerIdType>
						  <body><![CDATA[<otherRequestDetails><passCode> ...</passCode></otherRequestDetails>]]></body>
						  <submissionTime>{DateTime.Now}</submissionTime>
								</ns2:request>
							 </soapenv:Body>
						  </soapenv:Envelope>";

            return payload;
        }

        private string BuildGetCardTypePayload(string accountNumber)
        {
            var payload = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:soap=""http://soap.request.manager.redbox.stanbic.com/"">
				<soapenv:Header/>
				<soapenv:Body>
				<soap:request>
				<channel>REDBOX-OMNIFLOW</channel>
				<type>GET_CARD_TYPE</type>
				<customerId>{accountNumber}</customerId>
				<customerIdType>ACCOUNT_NUMBER</customerIdType>
				<reqTranId>{Util.TimeStampCode()}</reqTranId>
				<body/>
				</soap:request>
				</soapenv:Body>
				</soapenv:Envelope>";

            return payload;
        }

        #endregion private_helpers
    }
}