using QSDataUpdateAPI.Domain.Services;
using QSDataUpdateAPI.Domain;
using QSDataUpdateAPI.Core.Interfaces.Services.Helpers.Redbox;
using System;
using System.Threading.Tasks;
using QSDataUpdateAPI.Domain.Services.Helpers;
using QSDataUpdateAPI.Domain.Models.Requests.Redbox;

namespace QSDataUpdateAPI.Domain.Services.RedboxProxies
{
    public class OtpServiceProxy : IRedboxOtpServiceProxy
    {
        readonly IAppLogger _logger;
        readonly ISoapRequestHelper _soapRequestHelper;
        readonly IRedboxRequestManagerProxy _requestManagerProxy;

        public OtpServiceProxy(IAppLogger logger, ISoapRequestHelper soapRequestHelper, IRedboxRequestManagerProxy requestManagerProxy)
        {
            _logger = logger;
            _soapRequestHelper = soapRequestHelper;
            _requestManagerProxy = requestManagerProxy;
        }
        public async Task<(string responseCode, string responseDescription)> InitiateOtp(RedboxOtpRequest request)
        {
            try
            {
                var requestPayload = BuildOtpRequestPayload(request);
                var response = await _soapRequestHelper.SoapCall(requestPayload, "initiateOTPRequest", Constants.WS_Redbox_ChannelsService);
                if (response.ResponseCode == "00" || response.ResponseCode == "000")
                {
                    return Util.ParseRedboxGenericResponse(response.ResponseDescription);
                }
                return (response.ResponseCode, response.ResponseDescription);

            }
            catch (Exception exception)
            {
                _logger.Error(exception.Message, ex: exception);
                return ("9X9", exception.Message);
            }
        }

        public async Task<(string responseCode, string responseDescription)> VerifyOtp(RedboxOtpVerificationRequest request)
        {
            try
            {
                var requestPayload = BuildOtpVerificationRequestPayload(request);
                var response = await _soapRequestHelper.SoapCall(requestPayload, "validateOTPRequest", Constants.WS_Redbox_ChannelsService);
                if (response.ResponseCode == "00" || response.ResponseCode == "000")
                {
                    return Util.ParseRedboxGenericResponse(response.ResponseDescription);
                }
                return (response.ResponseCode, response.ResponseDescription);
            }
            catch (Exception exception)
            {
                _logger.Error(exception.Message, ex: exception);
                return ("9X9", exception.Message);
            }
        }


        public async Task<(string responseCode, string responseDescription, string otpReference)> InitiateOtpReqManager(string accountNumber)
        {
            try
            {
                var requestPayload = BuildReqManagerInitiateOtpRequestPayload(accountNumber);
                var response = await _requestManagerProxy.Post<string>(requestPayload);
                if (response.ResponseCode == "00" || response.ResponseCode == "000")
                {
                    var reference = Util.GetTagValue(response.Detail, "reference");
                    return ("00", response.ResponseDescription, reference);
                }
                return (response.ResponseCode, response.ResponseDescription, string.Empty);

            }
            catch (Exception exception)
            {
                _logger.Error($"ReqMngr Initiate Otp request failed with error -> {exception.Message}", ex: exception);
                throw;
            }
        }

        public async Task<(string responseCode, string responseDescription)> VerifyOtpReqManager(string accountNumber, string otp, string sourceReference)
        {
            try
            {
                var requestPayload = BuildReqManagerOtpVerificationRequestPayload(accountNumber, otp, sourceReference);
                var response = await _requestManagerProxy.Post<string>(requestPayload);  //_soapRequestHelper.SoapCall(requestPayload, "validateOTPRequest", Constants.WS_Redbox_ChannelsService);
                if (response.ResponseCode == "00" || response.ResponseCode == "000")
                {
                    return ("00", response.ResponseDescription);
                }
                return (response.ResponseCode, response.ResponseDescription);

            }
            catch (Exception exception)
            {
                _logger.Error($"ReqMngr Initiate Otp request failed with error -> {exception.Message}", ex: exception);
                throw;
            }
        }

        public async Task<(string responseCode, string responseDescription)> UpdatePhoneAndEmailOnFinacle(string accountNumber, string phoneNumber, string email)
        {
            try
            {
                var requestPayload = BuildUpdatePhoneAndEmailRequest(accountNumber, phoneNumber, email);
                var response = await _requestManagerProxy.Post<string>(requestPayload);
                if (response.ResponseCode == "00" || response.ResponseCode == "000")
                {
                    return ("00", response.ResponseDescription);
                }
                return (response.ResponseCode, response.ResponseDescription);

            }
            catch (Exception exception)
            {
                _logger.Error($"ReqMngr Update phone number and email request failed with error -> {exception.Message}", ex: exception);
                throw;
            }
        }

        #region private_helpers
        private string BuildOtpVerificationRequestPayload(RedboxOtpVerificationRequest request)
        {
            var payload = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:soap=""http://soap.channels.redbox.stanbic.com/"">
                              <soapenv:Header/>
                                <soapenv:Body>
                                  <soap:validateOTPRequest><userId>userId</userId>
                                   <CIF_ID>{request.CifId}</CIF_ID>
                                   <OTP_Type>SMS</OTP_Type>
                                   <sessionId>0000000000000</sessionId>
                                   <otp>{request.Otp}</otp>
                                   <reference>{request.OtpSourceReference}</reference>
                                   <isLogin>true</isLogin>
                                  </soap:validateOTPRequest>
                               </soapenv:Body>
                             </soapenv:Envelope>";
            return payload;
        }

        private string BuildUpdatePhoneAndEmailRequest(string accountNumber, string phoneNumber, string email)
        {
            var payload = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:soap=""http://soap.channels.redbox.stanbic.com/"">
                              <soapenv:Header/>
                                <soapenv:Body>
                                  <soap:validateOTPRequest><userId>userId</userId>
                                   <CIF_ID>{accountNumber}</CIF_ID>
                                   <OTP_Type>SMS</OTP_Type>
                                   <sessionId>0000000000000</sessionId>
                                   <otp>{phoneNumber}</otp>
                                   <reference>{email}</reference>
                                   <isLogin>true</isLogin>
                                  </soap:validateOTPRequest>
                               </soapenv:Body>
                             </soapenv:Envelope>";
            return payload;
        }

        private string BuildOtpRequestPayload(RedboxOtpRequest request)
        {
            var payload = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns: soap=""http://soap.channels.redbox.stanbic.com/"">
               <soapenv:Header />
                 <soapenv:Body>
                      <soap:initiateOTPRequest>
                     <userId>{request.UserId}</userId>
                     <CIF_ID>{request.CifId}</CIF_ID>
                     <OTP_Type></OTP_Type>
                     <returnNotificationDetails>true</returnNotificationDetails>
                     <sessionId>0000000000000</sessionId>
                     <reasonCode>{request.ReasonCode}</reasonCode>
                     <reasonDescription>{request.ReasonDescription}</reasonDescription>
                   </soap:initiateOTPRequest>
               </soapenv:Body>
             </soapenv:Envelope>";
            return payload;
        }

        private string BuildReqManagerInitiateOtpRequestPayload(string accountNumber)
        {
            var payload = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:soap=""http://soap.request.manager.redbox.stanbic.com/"">
                             <soapenv:Header/>
                                <soapenv:Body>
                                    <soap:request>
                                        <channel>MOBILE_APP</channel>
                                        <type>OTP_REQUEST</type>
                                        <customerId>{accountNumber}</customerId>
                                        <customerIdType>ACCOUNT_NUMBER</customerIdType>
                                        <body/>
                                    </soap:request>
                                </soapenv:Body>
                            </soapenv:Envelope>";
            return payload;
        }

        private string BuildReqManagerOtpVerificationRequestPayload(string accountNumber, string otp, string sourceReference)
        {
            var payload = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:soap=""http://soap.request.manager.redbox.stanbic.com/"">
                             <soapenv:Header/>
                                <soapenv:Body>
                                    <soap:request>
                                        <channel>MOBILE_APP</channel>
                                        <type>OTP_VALIDATION</type>
                                        <customerId>{accountNumber}</customerId>
                                        <customerIdType>ACCOUNT_NUMBER</customerIdType>
                                        <body>{otp}B{sourceReference}</body>
                                    </soap:request>
                                </soapenv:Body>
                            </soapenv:Envelope>";
            return payload;
        }



        #endregion
    }
}
