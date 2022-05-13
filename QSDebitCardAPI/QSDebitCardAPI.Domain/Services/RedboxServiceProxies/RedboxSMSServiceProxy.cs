using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using QSDataUpdateAPI.Domain.Services;
using QSDataUpdateAPI.Domain;
using Newtonsoft.Json;
using QSDataUpdateAPI.Domain.Services.Helpers;
using QSDataUpdateAPI.Domain.Models.Requests;

namespace QSDataUpdateAPI.Domain.Services.RedboxProxies
{
    public class RedboxSMSServiceProxy: IRedboxSMSService
    {
        readonly IAppLogger _logger;
        readonly IAppSettings _configSettings;
        public RedboxSMSServiceProxy(IAppLogger logger, IAppSettings settings) {
            _configSettings = settings;
            _logger = logger;
        }
        public async Task<BaseRedboxResponse> SendSMSAsync(string phoneNumber, string message, string acctNumber = "")
        {
            try {
                phoneNumber = FormatAsNigerianPhoneNumber(phoneNumber);
                var redboxSMsServiceUrl = _configSettings.GetString("AppSettings:RedboxSMSSvc");
                var requestMsg = GetSMSXmlPayload(phoneNumber, message, acctNumber);
                var requestResponse = await SoapCall(requestMsg, "", redboxSMsServiceUrl);
                return requestResponse;
            }
            catch(Exception exception)
            {
                _logger.Error($"Error occured while sending SMS to {phoneNumber} -> {exception}", exception);
                throw;
            }
        }

        public string FormatAsNigerianPhoneNumber(string phoneNumberString)
        {
            if (string.IsNullOrEmpty(phoneNumberString) || phoneNumberString.Length < 10 || phoneNumberString.StartsWith("234"))
                return phoneNumberString;
            if (phoneNumberString.StartsWith("0"))
                return new StringBuilder("234").Append(phoneNumberString.Substring(1)).ToString();
            if (phoneNumberString.StartsWith("+234"))
                return phoneNumberString.Substring(1);
            return phoneNumberString;
        }

        private string GetSMSXmlPayload(string mobile, string message, string acctNumber="")
        {
            string smsCostCenter = _configSettings.GetString("AppSettings:RedboxSMSCostCenter");
            var xmlPayload = $@"<SMSRequest>
                                  <SMSList>
                                    <SMS>
                                      <RecipientList>
                                        <RecipientMobileNumber>{mobile}</RecipientMobileNumber>
                                      </RecipientList>
                                      <SenderId></SenderId>
                                      <UseSenderId>0</UseSenderId>
                                      <AccountNumber>{acctNumber}</AccountNumber>
                                      <CostCentre>{smsCostCenter}</CostCentre>
                                      <ChargeCustomer>0</ChargeCustomer>
                                      <Message>{message}</Message>
                                      <EntityCode>test</EntityCode>
                                      <UseEntitySpecificGateway>0</UseEntitySpecificGateway>
                                      <IsSecuredFlag>0</IsSecuredFlag>
                                    </SMS>
                                  </SMSList>
                                </SMSRequest>";
            return xmlPayload.Replace("\n", "").Replace("\r","").Trim();
        }
        private async Task<BaseRedboxResponse> SoapCall(string soapRequest, string soapAction, string url)
        {
            var k = new BaseRedboxResponse("99", "Init");

            var reqId = $"{soapAction}_{Util.TimeStampCode()}";
            _logger.Info($"{soapAction} API REQ: {reqId}\n{soapRequest}");
            try
            {
                var uri = new Uri(url);
                var baseurl = uri.Scheme + "://" + uri.Authority;
                var path = uri.PathAndQuery;
                //do  web client call for soap 
                var client = new HttpClient
                {
                    BaseAddress = new Uri(baseurl)
                };
                var authParams = GetAuthAndModuleId();
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;
                
                // 
                client.DefaultRequestHeaders.Add("SOAPAction", soapAction);
                client.DefaultRequestHeaders.Add("module_id", $"{authParams.moduleId}");
                client.DefaultRequestHeaders.Add("authorization", $"basic {authParams.authId}");
                HttpResponseMessage responseMessage = null;
                // client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
                HttpContent contentPost = new StringContent(soapRequest, Encoding.Default, "text/xml");
                responseMessage = await client.PostAsync(url, contentPost);
                if (responseMessage.IsSuccessStatusCode)
                {
                    var resp = await responseMessage.Content.ReadAsStringAsync();
                    k = new BaseRedboxResponse("000", resp);
                }
                else
                {
                    var resp = await responseMessage.Content.ReadAsStringAsync();
                    k = new BaseRedboxResponse("9XX", resp);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex:ex);
                k = new BaseRedboxResponse("9XX", ex.Message);
            }

            var responseLogMsg = $"{soapAction} API RESP: {reqId} -> {JsonConvert.SerializeObject(k)}";
            _logger.Info(responseLogMsg);
            return k;
        }

        private (string moduleId, string authId) GetAuthAndModuleId()
        {
            string _modId = _configSettings.GetString("AppSettings:ModuleId");
            string _authId = _configSettings.GetString("AppSettings:AuthorizationId");
            return (_modId, _authId);
        }


        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return "0".Trim().Length < 5;
        }


    }
}