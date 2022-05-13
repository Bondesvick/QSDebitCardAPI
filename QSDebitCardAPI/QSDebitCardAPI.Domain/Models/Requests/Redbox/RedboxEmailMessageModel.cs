using System.Collections.Generic;

namespace QSDataUpdateAPI.Domain.Models.Requests.Redbox {
    public class RedboxEmailMessageModel
    {
        public string FromAddress { get; set; } = "redboxadmin@stanbicibtc.com";
        public string ToAddress { get; set; }
        public string CCAddresss { get; set; }
        public string BCc { get; set; }
        public List<RedboxEmailAttachment> Attachments { get; set; }
        public string Subject { get; set; }
        public string ContentType { get; set; } = "text/html";
        public string MailBody { get; set; }

    }

    public class RedboxEmailAttachment
    {
        public string AttachmentId { get; set; }
        public string AttachmentName { get; set; }
        public string AttachmentContentType { get; set; }
        public string Base64EncodedAttachmentData { get; set; }

    }
}