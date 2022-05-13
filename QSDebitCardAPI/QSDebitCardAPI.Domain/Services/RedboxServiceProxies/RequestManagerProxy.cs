using QSDataUpdateAPI.Domain.Services;
using QSDataUpdateAPI.Domain;
using QSDataUpdateAPI.Core.Interfaces.Services.Helpers.Redbox;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using QSDataUpdateAPI.Domain.Services.Helpers;
using QSDataUpdateAPI.Domain.Models.Requests;

namespace QSDataUpdateAPI.Domain.Services.RedboxProxies
{
    public class RedboxRequestManagerProxy : IRedboxRequestManagerProxy
    {
        private readonly ISoapRequestHelper _soapRequestHelper;
        private readonly IAppLogger _logger;
        private readonly IAppSettings _configSettings;

        public RedboxRequestManagerProxy(ISoapRequestHelper soapRequestHelper, IAppLogger logger, IAppSettings configSettings)
        {
            _soapRequestHelper = soapRequestHelper;
            _logger = logger;
            _configSettings = configSettings;
        }

        public async Task<BaseRequestManagerResponse<T2>> Post<T2>(string xmlReqMgrPayload, string module = "1", string authId = "1") where T2 : class
        {
            var resp = new BaseRequestManagerResponse<T2>();
            try
            {
                var reqResponse = await _soapRequestHelper.SoapCall(xmlReqMgrPayload, "\"treat\"", _configSettings.GetString("AppSettings:RedboxReqMngr"));
                if (reqResponse.ResponseCode != "000")
                {
                    resp.ResponseCode = reqResponse.ResponseCode;
                    resp.ResponseDescription = reqResponse.ResponseDescription.Contains("faultstring") ? Util.GetXmlTagValue(reqResponse.ResponseDescription, "faultstring") : reqResponse.ResponseDescription;
                }
                else
                {
                    resp.ResponseCode = Util.GetXmlTagValue(reqResponse.ResponseDescription, "responseCode", ignoreCase: false);
                    resp.ResponseDescription = Util.GetXmlTagValue(reqResponse.ResponseDescription, "responseDescription", ignoreCase: false);
                    resp.Detail = GetReqManagerResponseDetail(reqResponse.ResponseDescription);
                }
                _logger.Info(JsonConvert.SerializeObject(resp));
                if (resp.ResponseCode == "000" || resp.ResponseCode == "00")
                    resp.SetModel(_logger);
                else
                {
                    resp.DefaultModel();
                }
            }
            catch (Exception ex)
            {
                var _ex = new Exception("Redbox ReqManager API CALL FAILED:", ex);
                _logger.Error(ex.Message, ex: ex);
                throw _ex;
            }
            return resp;
        }

        private string GetReqManagerResponseDetail(string responseDescription)
        {
            try
            {
                var detailRespPart = Util.GetTagValue(responseDescription, "detail");
                if (detailRespPart.Contains("<![CDATA["))
                    detailRespPart = detailRespPart.Replace("<![CDATA[", "");
                if (detailRespPart.Contains("]]>"))
                    detailRespPart = detailRespPart.Replace("]]>", "");
                return detailRespPart.Trim();
            }
            catch (Exception exception)
            {
                _logger.Error($"Parsing request manager details failed error -> {exception}", exception);
                return string.Empty;
            }
        }

        /*
        private string GetRequestManagerPayload<T1>(BaseRequestMgrRequestPayload<T1> req, string prefix = "", string namespace_ = "", bool _header = false, bool cdata = false)
        {
            try
            {
                if (string.IsNullOrEmpty(prefix) && string.IsNullOrEmpty(namespace_))
                    req.SetBody(true);
                else
                    req.SetBody(prefix, namespace_, _header, cdata);
                string payload = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:soap=""http://soap.request.manager.redbox.stanbic.com/"">
                               <soapenv:Header/>
                                <soapenv:Body>
                                    <soap:request>
                                        <reqTranId>{req.ReqTranId}</reqTranId>
                                        <channel>{req.Channel}</channel>
                                        <type>{req.Type}</type>
                                        <customerId>{req.CustomerId}</customerId>
                                        <customerIdType>{req.CustomerIdType}</customerIdType>
                                        <submissionTime>{req.SubmissionTime}</submissionTime>
                                        <body>
                                            {req.Body}
                                        </body>
                                     </soap:request>
                               </soapenv:Body>
                              </soapenv:Envelope>";
                return payload;
            }
            catch (Exception exception)
            {
                throw;
            }
        }
        */

        public async Task<BaseRedboxResponse> MockSoapCall()
        {
            var responseSample = await File.ReadAllTextAsync(Path.Combine(GetDocumentsFolder(), "sample-response.xml"));
            return new BaseRedboxResponse("000", responseSample);
        }

        private static string GetDocumentsFolder()
        {
            try
            {
                var documentDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))
                    .GetDirectories().FirstOrDefault(d => d.Name.ToLower().Contains("documents"));
                if (documentDir == null)
                    throw new DirectoryNotFoundException("Cannot find documents folder");
                return Path.GetFullPath(documentDir.FullName);
            }
            catch (Exception exception)
            {
                throw;
            }
        }
    }
}