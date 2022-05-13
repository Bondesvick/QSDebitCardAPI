using QSDataUpdateAPI.Domain.Models.Requests.IdentityVerification;
using QSDataUpdateAPI.Domain.Models.Response.IdentityVerification;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QSDataUpdateAPI.Domain.Services.Interfaces
{
    public interface IIdVerificationService
    {
        Task<(IdVerificationResponse, HttpStatusCode)> VerifyId(IdVerificationRequest idVerificationRequest);
    }
}
