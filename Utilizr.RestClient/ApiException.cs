

namespace Utilizr.Rest.Client
{
    public class ApiException : Exception
    {
        public int StatusCode { get; set; }
        public string? FullResponse { get; set; }

        public ApiException(int statusCode, string? message, string? fullResponse) : base(message)
        {
            StatusCode = statusCode;
            FullResponse = fullResponse;
        }

        public override string ToString()
        {
            return string.Format("[ApiException: StatusCode={0}, Message={1}]", StatusCode, Message);
        }
    }
}
