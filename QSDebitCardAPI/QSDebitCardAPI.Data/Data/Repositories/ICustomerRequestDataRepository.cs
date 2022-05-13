using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using QSDataUpdateAPI.Core.Domain.Entities;

namespace QSDataUpdateAPI.Data.Repositories
{
    public interface ICustomerRequestDataRepository
    {
        Task<CustomerRequest> GetCustomerRequestById(long id);
        Task<CustomerRequest> UpdateCustomerRequest(CustomerRequest customerRequest);
        Task<IEnumerable<CustomerRequest>> GetCustomerRequests();
        Task<IEnumerable<CustomerRequest>> GetCustomerRequests(Func<CustomerRequest, bool> filter);
        Task<IEnumerable<CustomerRequest>> GetPagedCustomerRequests(long start, long count, Func<CustomerRequest, bool> predicate);
        //Task<RequestTypeStatusSummaryDTO> GetTypedRequestStatusSummary(string requestType, string sapId);
    }
}
