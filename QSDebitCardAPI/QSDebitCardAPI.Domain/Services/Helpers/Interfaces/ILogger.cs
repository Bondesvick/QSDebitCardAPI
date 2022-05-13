using System;
using System.Threading.Tasks;

namespace QSDataUpdateAPI.Domain.Services.Helpers
{
    public interface IAppLogger
    {
        void Info(string message, object data = null);
        void Warn(string message, object data = null);
        void Error(string message, object data = null, Exception ex = null);
        Task InfoAsync(string message, object data = null);
        Task WarnAsync(string message, object data = null);
        Task ErrorAsync(string message, object data = null, Exception ex = null);
    }
}
