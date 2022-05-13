using QSDataUpdateAPI.Domain.Models.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSDataUpdateAPI.Domain.Services.Helpers
{
    public interface ISoapRequestHelper
    {
        Task<BaseRedboxResponse> SoapCall(string soapRequest, string soapAction, string url, string moduleId = "", string authId = "", string contenttype = "text/xml");

    }
}
