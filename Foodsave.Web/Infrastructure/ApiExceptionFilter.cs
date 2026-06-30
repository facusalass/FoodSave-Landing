using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Foodsave.Web.Infrastructure
{
    public class ApiExceptionFilter : IExceptionFilter
    {
        private readonly IHostEnvironment _env;
        private readonly ILogger<ApiExceptionFilter> _logger;

        public ApiExceptionFilter(
            IHostEnvironment env,
            ILogger<ApiExceptionFilter> logger)
        {
            _env = env;
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            if (!context.HttpContext.Request.Path.StartsWithSegments("/api"))
                return;

            _logger.LogError(
                context.Exception,
                "Error no controlado en {Path}",
                context.HttpContext.Request.Path);

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Error interno del servidor.",
                Detail = _env.IsDevelopment() ? context.Exception.ToString() : null,
                Instance = context.HttpContext.Request.Path
            };

            context.Result = new ObjectResult(problem)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };

            context.ExceptionHandled = true;
        }
    }
}
