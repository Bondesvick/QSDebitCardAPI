using QSDataUpdateAPI.Domain.Models.Requests;
using System.Threading.Tasks;

namespace QSDataUpdateAPI.Domain.Services
{
    public interface IRedboxSMSService
    {
        Task<BaseRedboxResponse> SendSMSAsync(string phoneNumber, string message, string acctNumber = "");
    }
}
