using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QSDataUpdateAPI.Domain.Services.Helpers
{
    public interface IAuditLogService
    {
        Task<bool> AuditLog(string accountNo, string requestType, string methodName, string actionDescription, string ipAddress, string computerName);
    }
}
