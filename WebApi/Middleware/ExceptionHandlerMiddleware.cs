using Commons.Classes;
using Models.ViewModels;
using Newtonsoft.Json;
using System.Data;
using System.Net;

namespace WebApi.CustomExceptionMiddleware
{
    public class ExceptionHandlerMiddleware : ExceptionMiddleware
    {
        public ExceptionHandlerMiddleware(RequestDelegate next) : base(next)
        {
        }

        public override (HttpStatusCode code, string message) GetResponse(Exception exception)
        {
           
            HttpStatusCode code;
            switch (exception)
            {
                case KeyNotFoundException
                    or FileNotFoundException
                    or EntryPointNotFoundException:

                    code = HttpStatusCode.NotFound;
                    break;
                case DuplicateNameException:
                    code = HttpStatusCode.Conflict;
                    break;
                case UnauthorizedAccessException:
                    //or ExpiredPasswordException
                    //or UserBlockedException:
                    code = HttpStatusCode.Unauthorized;
                    break;
                case
                     //CreateUserException
                     //    or ResetPasswordException
                     //OrderValidatorException  --custome exception
                     ArgumentException
                    or InvalidOperationException:
                    code = HttpStatusCode.BadRequest;
                    break;
                default:
                    code = HttpStatusCode.InternalServerError;
                    break;
            }
            //var localizationKey = exception.Data[LocalizationKey]?.ToString() ?? LocalizerKeys.GeneralError;

            return (code, JsonConvert.SerializeObject(new APIResponse
            {
                Errors = new()
            {
                exception.Message
            },
                Status = code
            }));
        }
    }
}
