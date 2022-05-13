
using Microsoft.Extensions.Configuration;
using QSDataUpdateAPI.Core.Domain.Entities;
using QSDataUpdateAPI.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QSDataUpdateAPI.Domain.Services.Helpers
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IRepository<Audit, long> _auditLogRepository;
        readonly IAppLogger _logger;
        readonly IConfiguration _config;
        public AuditLogService(IRepository<Audit, long> auditLogRepo,  IConfiguration configuration, IAppLogger logger)
        {
            _auditLogRepository = auditLogRepo;
            _config = configuration;
            _logger = logger;
        }
        public async Task<bool> AuditLog(string accountNo, string requestType, string methodName, string actionDescription, string ipAddress, string computerName)
        {
            try
            {
                Audit audit = new Audit();
                audit.ActionBy = accountNo;
                audit.ActionDescription = actionDescription;
                audit.AuditDateTime = DateTime.Now;
                audit.ComputerName = computerName;
                audit.Hash = Cryptography.EncryptPhrase(accountNo+requestType+methodName);
                audit.IPAddress = ipAddress;
                audit.Id = Guid.NewGuid();
                audit.Method = methodName;
                audit.RequestType = requestType;
                await _auditLogRepository.AddItem(audit);
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, "AuditLog");
            }
            return await Task.FromResult(false);
        }
    }
}
