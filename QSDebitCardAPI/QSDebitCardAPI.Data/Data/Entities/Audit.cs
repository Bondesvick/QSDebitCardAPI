using System;
using System.Collections.Generic;
using System.Text;

namespace QSDataUpdateAPI.Core.Domain.Entities
{
    public class Audit
    {
        public Guid Id { get; set; }
        public string ActionBy { get; set; }
        public string RequestType { get; set; }
        public string Method { get; set; }
        public string IPAddress { get; set; }
        public string ComputerName { get; set; }
        public DateTime AuditDateTime { get; set; }
        public string ActionDescription { get; set; }
        public string Hash { get; set; }
    }
}
