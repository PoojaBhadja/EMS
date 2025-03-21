using Commons.Enums;
using Newtonsoft.Json;
using System.Net;

namespace Commons.Classes
{
    public class APIResponse
    {
        public bool Success { get { return this.Status == HttpStatusCode.OK; } }
        [JsonIgnore]
        public HttpStatusCode Status { get; set; }
        public int StatusCode { get { return Convert.ToInt32(Status); } }
        public string StatusText { get { return Convert.ToString(Status); } }
        public string SuccessMessage { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
    public class APIResponse<T> : APIResponse
    {
        public T Result { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
