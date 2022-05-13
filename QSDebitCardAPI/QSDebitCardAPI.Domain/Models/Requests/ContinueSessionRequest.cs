using System;
using System.Collections.Generic;
using System.Text;

namespace QSDataUpdateAPI.Domain.Models.Requests
{
    public class ContinueSessionRequest
    {
        public string CaseId { get; set; }
        public string AccountNumber { get; set; }
    }
}
