using Commons.Classes;
using Microsoft.Extensions.Logging;
using Models.ViewModels;
using Newtonsoft.Json;
using System.Data;
using System.Net;
using System.Reflection;
using System.Text.Json;

namespace WebApi.CustomExceptionMiddleware
{
    public abstract class ExceptionMiddleware 
    {
        //private static readonly ILogger Logger = Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType).Enrich();

        private readonly RequestDelegate _next;
        //public static string LocalizationKey => "LocalizationKey";

        //private readonly ILoggerManager _logger;
        public abstract (HttpStatusCode code, string message) GetResponse(Exception exception);
        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                // log the error
                //Logger.Error(exception, "error during executing {Context}", context.Request.Path.Value);
                var response = context.Response;
                response.ContentType = "application/json";

                // get the response code and message
                var (status, message) = GetResponse(exception);
                response.StatusCode = (int)status;
                await response.WriteAsync(message);
            }
        }

        //public ExceptionMiddleware(RequestDelegate next/*, ILoggerManager logger*/)
        //{
        //    //_logger = logger;
        //    _next = next;
        //}
        //public async Task InvokeAsync(HttpContext httpContext)
        //{
        //    try
        //    {
        //        await _next(httpContext);
        //    }
        //    catch (AccessViolationException avEx)
        //    {
        //        //_logger.LogError($"A new violation exception has been thrown: {avEx}");
        //        await HandleExceptionAsync(httpContext, avEx);
        //    }
        //    catch (Exception ex)
        //    {
        //        //_logger.LogError($"Something went wrong: {ex}");
        //        await HandleExceptionAsync(httpContext, ex);
        //    }

        //}
        //private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        //{
        //    //context.Response.ContentType = "application/json";
        //    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        //    var message = exception switch
        //    {
        //        AccessViolationException => "Access violation error from the custom middleware",
        //        _ => "Internal Server Error from the custom middleware."
        //    };
        //    List<string> errors = new()
        //                {
        //                   exception.Message
        //                };

        //    await context.Response.WriteAsync(new ErrorDetails()
        //    {
        //        StatusCode = context.Response.StatusCode,
        //        Message = exception.Message
        //    }.ToString());
        //}

    }
}
