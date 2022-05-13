using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using QSDataUpdateAPI.Core.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QSDataUpdateAPI.Data.Repositories;

namespace QSDataUpdateAPI.Data.Data.Repositories
{
    public class CustomerRequestDataRepository : BaseEFRepository<CustomerRequest, long>, ICustomerRequestDataRepository
    {
        private readonly ILogger<CustomerRequestDataRepository> _logger;
        private readonly IConfiguration _config;

        public CustomerRequestDataRepository(QuickServiceDbContext c, ILogger<CustomerRequestDataRepository> logger, IConfiguration config) : base(c)
        {
            _logger = logger;
            _config = config;
        }

        public async Task<CustomerRequest> GetCustomerRequestById(long id)
        {
            try
            {
                using (var dbContext = new QuickServiceDbContext())
                {
                    var requestWithNavProps = dbContext.CustomerRequest
                                                .Include(p => p.DebitCardDetails)
                                                .ThenInclude(p => p.Documents).Where(x => x.Id == id);
                    return await requestWithNavProps.FirstOrDefaultAsync();
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"An LogError occured while retrieving paged customer requests", exception);
                throw;
            }
        }

        public async Task<CustomerRequest> UpdateCustomerRequest(CustomerRequest customerRequest)
        {
            try
            {
                using (var dbContext = new QuickServiceDbContext())
                {
                    //dbContext.Entry(customerRequest).State = EntityState.Modified;
                    var requestWithNavProps = dbContext.CustomerRequest.Update(customerRequest);
                    await dbContext.SaveChangesAsync();

                    return requestWithNavProps.Entity;
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"An LogLogError occured while retrieving paged customer requests", exception);
                throw;
            }
        }

        public async Task<IEnumerable<CustomerRequest>> GetCustomerRequests()
        {
            try
            {
                using (var dbContext = new QuickServiceDbContext())
                {
                    var requestWithNavProps = dbContext.CustomerRequest
                                                .Include(p => p.DebitCardDetails)
                                                .ThenInclude(p => p.Documents);
                    return await requestWithNavProps.ToListAsync();
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"An LogLogError occured while retrieving paged customer requests", exception);
                throw;
            }
        }

        public async Task<IEnumerable<CustomerRequest>> GetCustomerRequests(Func<CustomerRequest, bool> filter)
        {
            try
            {
                using (var dbContext = new QuickServiceDbContext())
                {
                    var requestWithNavProps = dbContext.CustomerRequest
                                                .Include(p => p.DebitCardDetails)
                                                .ThenInclude(p => p.Documents);
                    return await Task.Run(() => requestWithNavProps.Where(filter).ToList());
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"An LogLogError occured while retrieving paged customer requests", exception);
                throw;
            }
        }

        public async Task<IEnumerable<CustomerRequest>> GetPagedCustomerRequests(long start, long count, Func<CustomerRequest, bool> predicate)
        {
            try
            {
                using (var dbContext = new QuickServiceDbContext())
                {
                    var requestWithNavProps = dbContext.CustomerRequest
                                                .Include(p => p.DebitCardDetails)
                                                .ThenInclude(p => p.Documents);
                    var res = await Task.Run(() => requestWithNavProps.Where(predicate).Skip((int)start).Take((int)count).ToList());
                    return res;
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"An LogError occured while retrieving paged customer requests", exception);
                throw;
            }
        }

        private DataSet GetDataSet(SqlConnection connection, string storedProcName, params SqlParameter[] parameters)
        {
            using var command = new SqlCommand(storedProcName, connection) { CommandType = CommandType.StoredProcedure };
            command.Parameters.AddRange(parameters);

            var result = new DataSet();
            using var dataAdapter = new SqlDataAdapter(command);
            dataAdapter.Fill(result);

            return result;
        }
    }
}