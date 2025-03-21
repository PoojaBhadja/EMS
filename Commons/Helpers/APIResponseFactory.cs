using Commons.Classes;
using Commons.Enums;
using Models.ViewModels;
using System.Net;

namespace Commons.Helpers
{
    public static class APIResponseFactory
    {
        /// <summary>
        /// Generates a success response with a result.
        /// </summary>
        public static APIResponse<T> Success<T>(T result, HttpStatusCode status = HttpStatusCode.OK)
        {
            return new APIResponse<T>
            {
                Status = status,
                Result = result
            };
        }

        public static APIResponse Success(string successMessage, HttpStatusCode status = HttpStatusCode.OK)
        {
            return new APIResponse
            {
                SuccessMessage = successMessage,
                Status = status,
            };
        }

        /// <summary>
        /// Generates a failure response with error messages.
        /// </summary>
        public static APIResponse Failure(HttpStatusCode status, string errorMessage)
        {
            return new APIResponse
            {
                Status = status,
                Errors = new List<string> { errorMessage }
            };
        }

        /// <summary>
        /// Generates a failure response with multiple error messages.
        /// </summary>
        public static APIResponse Failure(HttpStatusCode status, List<string> errors)
        {
            return new APIResponse
            {
                Status = status,
                Errors = errors ?? new List<string>()
            };
        }

        public static APIResponse<T> Failure<T>(HttpStatusCode status, string errorMessage)
        {
            return new APIResponse<T>
            {
                Status = status,
                Errors = new List<string> { errorMessage }
            };
        }

        /// <summary>
        /// Generates a failure response with multiple error messages.
        /// </summary>
        public static APIResponse<T> Failure<T>(HttpStatusCode status, List<string> errors)
        {
            return new APIResponse<T>
            {
                Status = status,
                Errors = errors ?? new List<string>()
            };
        }
    }
}
