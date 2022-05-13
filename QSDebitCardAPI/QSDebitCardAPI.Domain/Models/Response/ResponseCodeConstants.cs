using System;
using System.Text.RegularExpressions;

namespace QSDataUpdateAPI.Domain.Models
{
    public class ResponseCodeConstants
    {
        public static string SUCCESS {internal set; get;} = "00";
        public static string FAILURE {internal set; get;} = "99";
        public static string INTERNAL_EXCEPTION { internal set; get; } = "XX";
        public static string BAD_REQUEST { internal set; get; } = "400";
    }
}
