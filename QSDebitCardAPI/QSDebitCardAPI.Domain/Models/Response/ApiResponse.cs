using System;
using System.Text.RegularExpressions;

namespace QSDataUpdateAPI.Domain.Models.Response
{
    public class ApiResponse<T> where T : class
    {
        public string ResponseCode { get; set; }
        public string ResponseDescription { get; set; }
        public T Data { get; set; }

        public ApiResponse(){}
        public ApiResponse(string respCode, string respDesc)
        {
            ResponseCode = respCode;
            ResponseDescription = respDesc;
        }
        public ApiResponse(string respCode, string respDesc, T data)
        {
            ResponseCode = respCode;
            ResponseDescription = respDesc;
            Data = data;
        }
    }
}
