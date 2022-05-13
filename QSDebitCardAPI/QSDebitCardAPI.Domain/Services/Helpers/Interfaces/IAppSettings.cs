using System;

namespace QSDataUpdateAPI.Domain.Services.Helpers
{
    public interface IAppSettings
    {
        object Get(string key);
        string GetString(string key);
        int GetInt(string key);
        bool GetBool(string key);
        long GetLong(string key);
    }
}