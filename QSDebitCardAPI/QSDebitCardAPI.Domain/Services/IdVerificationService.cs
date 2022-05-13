using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QSDataUpdateAPI.Domain.Models.Requests.IdentityVerification;
using QSDataUpdateAPI.Domain.Models.Response.IdentityVerification;
using QSDataUpdateAPI.Domain.Services.Helpers;
using QSDataUpdateAPI.Domain.Services.Interfaces;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QSDataUpdateAPI.Domain.Services
{
    public class IdVerificationService : IIdVerificationService
    {
        private readonly ILogger<IdVerificationService> _logger;
        readonly IAppSettings _configSettings;
        private readonly ISoapRequestHelper _soapRequestHelper;
        public IdVerificationService(ILogger<IdVerificationService> logger, IAppSettings configSettings, ISoapRequestHelper soapRequestHelper)
        {
            _logger = logger;
            _configSettings = configSettings;
            _soapRequestHelper = soapRequestHelper;

        }
        public async Task<(IdVerificationResponse, HttpStatusCode)> VerifyId(IdVerificationRequest idVerificationRequest)
        {
            try
            {
                var useSmileIdentity = _configSettings.GetInt("AppSettings:UseSmileIdentity");
                if (useSmileIdentity < 1)
                {
                    _logger.LogInformation($"ID validation: Smile Identity is turned off");
                    return (null, HttpStatusCode.OK);
                }

                var idVerificationUrl = _configSettings.GetString("AppSettings:IdVerificaitonUrl");
                idVerificationRequest.moduleId = _configSettings.GetString("AppSettings:ModuleId");
                idVerificationRequest.dob = DateTime.Now.ToString("yyyy-M-dd");

                switch (idVerificationRequest.idType)
                {
                    case "Nigeria Permanent Voter's Card (PVC)":
                        idVerificationRequest.idType = "Voter ID";
                        break;
                    case "Nigerian National Identity Card (NIMC)":
                        idVerificationRequest.idType = "NIN";
                        break;
                    case "Nigeria Drivers License":
                        idVerificationRequest.idType = "Drivers Licence";
                        break;
                    case "Nigerian International Passport":
                        idVerificationRequest.idType = "International Passport";
                        break;
                    default:
                        break;
                }

                var client = new RestClient(idVerificationUrl);

                var request = new RestRequest(Method.POST);


                var reqResponse = await _soapRequestHelper.SoapCall(JsonConvert.SerializeObject(idVerificationRequest), "", idVerificationUrl, "", "", "application/json");

                var result = JsonConvert.DeserializeObject<IdVerificationResponse>(reqResponse.ResponseDescription);

                // _logger.LogInformation($"ID validation response: ", result);
                if (reqResponse.ResponseCode == "000") { return (result, HttpStatusCode.OK); } else { return (result, HttpStatusCode.PreconditionFailed); }

            }
            catch (Exception ex)
            {
                _logger.LogError("An error occured while making a REST API request", ex);
                return (null, HttpStatusCode.InternalServerError);
            }

        }
    }
}
