using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace QSDebitCardAPI.Filters
{
    public class AuthSecretKeyFilter : IAsyncActionFilter
    {
        private IConfiguration _config;

        public AuthSecretKeyFilter(IConfiguration config)
        {
            _config = config;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue("X-Stack-Eb", out StringValues passedSecret))
            {
                context.Result = new BadRequestObjectResult("Invalid request");
                await Task.FromResult(false);
                return;
            }

            if (_config["Appsettings:X-Client-Secret"] != passedSecret.ToString())
            {
                context.Result = new BadRequestObjectResult("Unknown source");
                await Task.FromResult(false);
                return;
            }

            await next();
        }
    }
}