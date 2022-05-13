using System.Threading.Tasks;
using QSDataUpdateAPI.Domain.Models.Requests;
using QSDataUpdateAPI.Domain.Models.Requests.Redbox;

namespace QSDataUpdateAPI.Domain.Services.RedboxProxies
{
    public interface IRedboxEmailService
    {
        Task<BaseRedboxResponse> SendEmailAsync(RedboxEmailMessageModel mailMessage);
    }
}
