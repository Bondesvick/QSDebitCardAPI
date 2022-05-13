using QSDataUpdateAPI.Domain.Models.Requests;
using System.Threading.Tasks;

namespace QSDataUpdateAPI.Core.Interfaces.Services.Helpers.Redbox
{
    public interface IRedboxRequestManagerProxy
    {
        Task<BaseRequestManagerResponse<T2>> Post<T2>(string xmlReq, string module = "1", string authId = "1") where T2: class;
    }
}
