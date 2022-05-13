using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using QSDataUpdateAPI.Core.Interfaces.Services.Helpers.Redbox;
using QSDataUpdateAPI.Domain.Models;
using QSDataUpdateAPI.Domain.Models.Requests;
using QSDataUpdateAPI.Domain.Models.Requests.Redbox;
using QSDataUpdateAPI.Domain.Models.Response;
using QSDataUpdateAPI.Domain.Services;
using QSDataUpdateAPI.Domain.Services.Helpers;
using QSDataUpdateAPI.Filters;
using QSDebitCardAPI.Filters;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace QSDebitCardAPI.Controllers
{
    [Route("api/[controller]/dataUpdate")]
    [ServiceFilter(typeof(AuthSecretKeyFilter), Order = 1)]
    public class CustomerRequestController : Controller
    {
        private readonly ICustomerRequestService _requestService;
        private readonly IRedboxOtpServiceProxy _otpServiceProxy;
        private readonly IAppLogger _logger;
        private readonly IAuditLogService _auditLogService;

        public CustomerRequestController(ICustomerRequestService requestService, IAppLogger logger, IRedboxOtpServiceProxy otpServiceProxy, IAuditLogService auditLogService)
        {
            _requestService = requestService;
            _logger = logger;
            _otpServiceProxy = otpServiceProxy;
            _auditLogService = auditLogService;
        }

        [HttpPost("inititiateOtp")]
        public async Task<IActionResult> InitiateDataUpdateOtp([FromBody] RedboxOtpRequest request)
        {
            try
            {
                await _auditLogService.AuditLog(request.UserId, "Debit card", "InitiateDataUpdateOtp", "Initiating OTP: ", HttpContext.Connection.RemoteIpAddress.ToString(), Dns.GetHostName().ToString());
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception occured on  api/customerrequests/dataUpdate/initiateRequest -> {ex.Message}", ex);
                return Ok(new ApiResponse<object>(ResponseCodeConstants.INTERNAL_EXCEPTION, ex.Message));
            }

            try
            {
                if (!ModelState.IsValid)
                    return Ok(new ApiResponse<ModelStateDictionary>(ResponseCodeConstants.BAD_REQUEST, "Otp Request Payload is not valid", ModelState));
                var response = await _otpServiceProxy.InitiateOtpReqManager(request.UserId);
                if (response.responseCode == ResponseCodeConstants.SUCCESS)
                    return Ok(new ApiResponse<object>(ResponseCodeConstants.SUCCESS, response.otpReference));
                return
                    Ok(new ApiResponse<object>(response.responseCode, response.responseDescription));
            }
            catch (Exception exception)
            {
                _logger.Error($"Exception occured on api/customerrequests/dataUpdate/initiateRequest -> {exception.Message}", ex: exception);
                return Ok(new ApiResponse<object>(ResponseCodeConstants.INTERNAL_EXCEPTION, exception.Message));
            }
        }

        [HttpPost("submitRequest")]
        public async Task<IActionResult> SubmitAdditionalAccountOpeningRequest([FromBody] DebitCardRequest request)
        {
            try
            {
                await _auditLogService.AuditLog(request.AccountNumber, "Debit card", "SubmitAdditionalAccountOpeningRequest", "Submitting Debit card Request: ", HttpContext.Connection.RemoteIpAddress.ToString(), Dns.GetHostName().ToString());
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception occured on  api/customerrequests/dataUpdate/submitRequest -> {ex.Message}", ex);
                return Ok(new ApiResponse<object>(ResponseCodeConstants.INTERNAL_EXCEPTION, ex.Message));
            }

            try
            {
                if (!ModelState.IsValid)
                    return Ok(new ApiResponse<object>(ResponseCodeConstants.BAD_REQUEST, "Debit card request Payload is not valid", new { detail = GetModelErrors(ModelState) }));
                var response = await _requestService.VerifyAndSaveAdditionalAccountOpeningRequest(request);
                if (response.status)
                    return Ok(new ApiResponse<object>(ResponseCodeConstants.SUCCESS, response.statusMessage, new { detail = response.result }));
                return
                    //Ok(new ApiResponse<object>(ResponseCodeConstants.FAILURE, response.statusMessage, new { detail = response.result }));
                    Ok(new ApiResponse<object>(ResponseCodeConstants.FAILURE, response.statusMessage, new { response.statusMessage }));
            }
            catch (Exception exception)
            {
                _logger.Error($"Exception occured on api/customerrequests/dataUpdate/submitRequest -> {exception.Message}", ex: exception);
                return Ok(new ApiResponse<object>(ResponseCodeConstants.INTERNAL_EXCEPTION, exception.Message));
            }
        }

        [HttpPost("saveAndContinue")]
        public async Task<IActionResult> SaveAndContinueDataUpdateRequest([FromBody] DebitCardRequest request)
        {
            try
            {
                await _auditLogService.AuditLog(request.AccountNumber, "Debit card", "SaveAndContinueDataUpdateRequest", "Saving Debit card Request: ", HttpContext.Connection.RemoteIpAddress.ToString(), Dns.GetHostName().ToString());
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception occured on  api/customerrequests/dataUpdate/saveAndContinue -> {ex.Message}", ex);
                return Ok(new ApiResponse<object>(ResponseCodeConstants.INTERNAL_EXCEPTION, ex.Message));
            }

            try
            {
                if (!ModelState.IsValid)
                    return Ok(new ApiResponse<object>(ResponseCodeConstants.BAD_REQUEST, "Debit card request Payload is not valid", new { detail = GetModelErrors(ModelState) }));

                var response = await _requestService.SaveAndContinue(request, "INCOMPLETE");
                if (response.status)
                    return Ok(new ApiResponse<object>(ResponseCodeConstants.SUCCESS, response.statusMessage, new { detail = response.result }));
                return
                    Ok(new ApiResponse<object>(ResponseCodeConstants.FAILURE, response.statusMessage, new { detail = response.result }));
            }
            catch (Exception exception)
            {
                _logger.Error($"Exception occured on api/customerrequests/dataUpdate/saveAndContinue -> {exception.Message}", ex: exception);
                return Ok(new ApiResponse<object>(ResponseCodeConstants.INTERNAL_EXCEPTION, exception.Message));
            }
        }

        [HttpPost("verifyCaseId")]
        public async Task<IActionResult> VerifyCaseId([FromBody] ContinueSessionRequest request)
        {
            try
            {
                await _auditLogService.AuditLog(request.AccountNumber, "Debit card", "VerifyCaseId", "Verifying ticket ID: ", HttpContext.Connection.RemoteIpAddress.ToString(), Dns.GetHostName().ToString());
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception occured on  api/customerrequests/dataUpdate/verifyCaseId -> {ex.Message}", ex);
                return Ok(new ApiResponse<object>(ResponseCodeConstants.INTERNAL_EXCEPTION, ex.Message));
            }

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var response = await _requestService.VerifyCaseId(request.CaseId);

                if (response.status)
                {
                    if (response.result.CustomerReq.AccountNumber != request.AccountNumber)
                        return Ok(new ApiResponse<object>(ResponseCodeConstants.BAD_REQUEST, "The Session ID does not match the specified account number", null));

                    if (response.result.Submitted)
                        return Ok(new ApiResponse<object>(ResponseCodeConstants.BAD_REQUEST, "There is no ongoing Session that matches this ID", null));

                    return Ok(new ApiResponse<object>(ResponseCodeConstants.SUCCESS, response.statusMessage, new { detail = response.result }));
                }

                return
                    Ok(new ApiResponse<object>(ResponseCodeConstants.FAILURE, response.statusMessage, null));
            }
            catch (Exception exception)
            {
                _logger.Error($"Exception occured on api/customerrequests/dataUpdate/verifyCaseId -> {exception.Message}", ex: exception);
                return Ok(new ApiResponse<object>(ResponseCodeConstants.INTERNAL_EXCEPTION, exception.Message));
            }
        }

        #region privates

        /// <summary>
        /// Fleshes out ModelState errors for any model into a nicely simplified dictionary of property: errors []
        /// </summary>
        /// <param name="modelState"></param>
        /// <returns></returns>
        private IDictionary<string, string[]> GetModelErrors(ModelStateDictionary modelState)
        {
            return modelState.Where(key => key.Value.Errors.Count > 0)
                        .ToDictionary(
                            k => k.Key,
                            v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                            );
        }

        #endregion privates
    }
}