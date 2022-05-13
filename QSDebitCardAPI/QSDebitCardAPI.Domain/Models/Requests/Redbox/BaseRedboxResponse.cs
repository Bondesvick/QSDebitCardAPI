using QSDataUpdateAPI.Domain;
using QSDataUpdateAPI.Domain.Services.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QSDataUpdateAPI.Domain.Models.Requests
{
    public class BaseRedboxResponse
    {
        public string ResponseCode { get; set; }
        public string ResponseDescription { get; set; }

        public BaseRedboxResponse() { }
        public BaseRedboxResponse(string respCode, string respDescr)
        {
            ResponseCode = respCode;
            ResponseDescription = respDescr;
        }
    }

    public class BaseRequestManagerResponse<T> : BaseRedboxResponse where T : class
    {
        public string TrackingId { get; set; }
        public string Detail { get; set; }
        public T Model { get; set; }

        public void SetModel(IAppLogger logger = null)
        {
            try
            {
                Type myType = typeof(T);
                if (myType.FullName != "System.String" && !string.IsNullOrEmpty(Detail))
                {
                    logger?.Error($"Entered Model Deserialization phase -> {Detail}");
                    Model = Util.DeserializeXML<T>(Detail);
                    logger?.Info($"Post model deserialization -> {typeof(T)}");
                }
            }
            catch (Exception ex)
            {
                var _ex = new Exception("READ MODEL FROM REDBOX FAILED", ex);
                if (logger != null)
                    logger.Error($"Error occured while deserializing ReqMngr Response detail to type {typeof(T)}", ex: ex);
                Model = default(T);
            }
        }

        public void DefaultModel()
        {
            Model = default(T);
        }
        public BaseRequestManagerResponse() { }
        public BaseRequestManagerResponse(string respCode, string respDescr) : base(respCode, respDescr) { }
    }
}
