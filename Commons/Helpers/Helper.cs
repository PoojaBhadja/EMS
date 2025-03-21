using Commons.Classes;
using Commons.Enums;
using Models.ViewModels;
using System.Net;

namespace Commons.Helpers
{
    public static class Helper
    {
        private static readonly string hiddenPasswordString = "*******";

        /// <summary>
        /// Return API response along with result data
        /// </summary>
        /// <typeparam name="T">Type of Result Object</typeparam>
        /// <param name="result">Result Object</param>
        /// <param name="responseStatus">Response Status</param>
        /// <returns></returns>
        public static APIResponse<T> CreateApiResponse<T>(T result, HttpStatusCode responseStatus)
        {

            APIResponse<T> response = new APIResponse<T>();
            response.Status = responseStatus;
            response.Result = result;
            return response;
        }

        /// <summary>
        /// Return API response with error information
        /// </summary>
        /// <typeparam name="T">Type of Result Object</typeparam>
        /// <param name="responseStatus">Response Status</param>
        /// <param name="errors">List of Errors</param>
        /// <returns></returns>
        public static APIResponse<T> CreateApiResponse<T>(HttpStatusCode responseStatus, List<string> errors = null)
        {
            APIResponse<T> response = new APIResponse<T>();
            response.Status = responseStatus;
            response.Errors = errors;
            return response;
        }

        /// <summary>
        /// Return API response with error information
        /// </summary>
        /// <param name="responseStatus">Response Status</param>
        /// <param name="errors">List of Errors</param>
        /// <returns></returns>
        public static APIResponse CreateApiResponse(ResponseStatus responseStatus, List<string> errors = null)
        {
            APIResponse response = new APIResponse();
            //response.Status = responseStatus.ToString();
            response.Errors = errors;
            return response;
        }
    }
}
