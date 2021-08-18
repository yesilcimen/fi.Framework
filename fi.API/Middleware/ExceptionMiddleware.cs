using fi.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace fi.API
{
    internal class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ExceptionMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<ExceptionMiddleware>();
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                await HandleExceptionAsync(httpContext, e);
            }
        }

        private static Task HandleExceptionAsync(HttpContext httpContext, Exception ex)
        {
            ErrorModel errorModel;

            if (ex is not BaseException baseException)
            {
                HttpStatusCode statusCode;
                statusCode = ex switch
                {
                    ArgumentNullException or ArgumentOutOfRangeException or DivideByZeroException or IndexOutOfRangeException or InvalidCastException or NullReferenceException or OutOfMemoryException => HttpStatusCode.InternalServerError,
                    TimeoutException => HttpStatusCode.RequestTimeout,
                    _ => HttpStatusCode.BadRequest,
                };

                errorModel = new ErrorModel(new ErrorResult[] { new(ex.Message) }, (int)statusCode, ResponseMessageType.UnhandledException);
            }
            else
                errorModel = baseException.ErrorModel;

            return HandleAsync(httpContext, errorModel);
        }

        private static Task HandleAsync(HttpContext context, ErrorModel baseApiErrorResponse)
        {
            context.Response.ContentType = "application/json";
            var options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault };
            var json = JsonSerializer.Serialize(baseApiErrorResponse, options);
            context.Response.StatusCode = baseApiErrorResponse.Code;
            context.Response.Headers.Add("RequestId", context.TraceIdentifier);

            return context.Response.WriteAsync(json);
        }
    }
}
