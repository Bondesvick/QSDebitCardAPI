using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QSDataUpdateAPI.Domain.Models.Requests;
using QSDataUpdateAPI.Domain.Models.Requests.Redbox;
using QSDataUpdateAPI.Domain.Services.Helpers;

namespace QSDataUpdateAPI.Domain.Services.RedboxProxies
{
    public class RedboxEmailServiceProxy: IRedboxEmailService
    {
        private readonly ISoapRequestHelper _soapRequestHelper;
        readonly IAppLogger _logger;
        readonly IAppSettings _configSettings;
        public RedboxEmailServiceProxy(IAppLogger logger, IAppSettings settings, ISoapRequestHelper soapRequestHelper)
        {
            _soapRequestHelper = soapRequestHelper;
            _logger = logger;
            _configSettings = settings;
        }

        public async Task<BaseRedboxResponse> SendEmailAsync(RedboxEmailMessageModel mailMessage)
        {
            try {
                var payload = GetRedboxEmailRequestPayload(mailMessage);
                var requestResponse = await _soapRequestHelper.SoapCall(payload, "sendEmailMessage", _configSettings.GetString("AppSettings:RedboxEmailSvc"));
                return requestResponse;
            }
            catch (Exception exception)
            {
                _logger.Error($"Error occured while sending Email to {mailMessage.ToAddress} -> {exception.Message}", ex: exception);
                throw;
            }
        }

        private string GetRedboxEmailRequestPayload(RedboxEmailMessageModel mailMessage)
        {
            var payload = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:soap=""http://soap.messaging.outbound.redbox.stanbic.com/"">
                           <soapenv:Header />
                               <soapenv:Body> 
                               <soap:EMail>
                               <From>{mailMessage.FromAddress}</From>   
                               <To>{mailMessage.ToAddress}</To>        
                                  <Cc>{mailMessage.CCAddresss}</Cc>
                                     <BCc/>
                                     <Attachments>            
                                         {GetAttachmentsPayload(mailMessage.Attachments)}
                                    </Attachments> 
                                     <Subject>{mailMessage.Subject}</Subject>
                                     <ContentType>{mailMessage.ContentType}</ContentType>
                                     <Body>
                                        <![CDATA[{mailMessage.MailBody}]]></Body>
                                  </soap:EMail>
                                </soapenv:Body></soapenv:Envelope>";
            return payload;
        }

        private string GetAttachmentsPayload(List<RedboxEmailAttachment> attachements)
        {
            if(attachements == null || attachements.Count < 1)
                return string.Empty;
            var attachmentPayloads = new StringBuilder();
            foreach(var attachment in attachements)
            {
                attachmentPayloads.AppendLine($@"
                                    <Attachment>
                                       <AttachmentId>{attachment.AttachmentId}</AttachmentId>
                                       <AttachmentName>{attachment.AttachmentName}</AttachmentName>
                                       <AttachmentContentType>{attachment.AttachmentContentType}</AttachmentContentType>
                                       <AttachmentData>{attachment.Base64EncodedAttachmentData}</AttachmentData>
                                    </Attachment>");
            }
            return attachmentPayloads.ToString();
        }
    }
}
