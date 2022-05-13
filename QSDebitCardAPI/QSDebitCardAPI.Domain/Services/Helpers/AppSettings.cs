using System;
using QSDataUpdateAPI.Domain.Services.Helpers;
using Microsoft.Extensions.Configuration;

namespace QSDataUpdateAPI.Domain.Services.Helpers
{
    public class AppSettings : IAppSettings
    {
        readonly IConfiguration _configuration;

        public AppSettings(IConfiguration config)
        {
            _configuration = config;
        }
        public object Get(string key)
        {
            return _configuration[key];
        }

        public bool GetBool(string key)
        {
            bool value = false;
            bool.TryParse(_configuration[key], out value);
            return value;
        }

        public int GetInt(string key)
        {
            int value = 0;
            int.TryParse(_configuration[key], out value);
            return value;
        }

        public long GetLong(string key)
        {
            long value = 0;
            long.TryParse(_configuration[key], out value);
            return value;
        }

        public string GetString(string key)
        {
            return _configuration[key];
        }
    }
}