using Newtonsoft.Json;
using RestSharp;
using System.Net;
using System.Text.RegularExpressions;

namespace Utilizr.Rest.Client
{
    public interface IApiRequest
    {
        string EndpointLogStr { get; }
        public string Endpoint { get; }

        string MethodLogStr { get; }
        public Method Method { get; }

        public object? Body { get; set; }

        public object? Query { get; set; }

        Dictionary<string, string> Headers { get; set; }

        public bool LogRequest { get; }
    }

    public interface IApiRequest<TResponse> : IApiRequest
    {
        /// <summary>
        /// Raw RestSharp response instance.
        /// </summary>
        public RestResponse<TResponse>? Response { get; set; }

        /// <summary>
        /// Gets the object for request logging.
        /// Changing this object will have no effect on the data being sent.
        /// </summary>
        /// <returns>The new object for request logging.</returns>
        object GetObjectForRequestLogging();

        /// <summary>
        /// Optionally add any extra headers.
        /// </summary>
        Dictionary<string, string>? GetExtraRequestSpecificHeaders();

        /// <summary>
        /// Override post processing to perform internal tasks upon a successful response
        /// NOTE: this method is NOT async, do not write blocking code in this method
        /// </summary>
        void PostProcessing(TResponse response);

        /// <summary>
        /// Gets the object for response logging. Override this method if you need to remove sensitive data before logging
        /// Note, the changes you make on this response object only affect logging.
        /// </summary>
        /// <returns>The object for response logging.</returns>
        TResponse GetObjectForResponseLogging();

        /// <summary>
        /// Return an optional more detailed error message instead of the stardand HTTP status.
        /// </summary>
        /// <param name="statusCode">HTTP status code.</param>
        /// <param name="response">Types response for the attempted request.</param>
        /// <returns>Null for default behaviour, or optionally a more detailed error message.</returns>
        string? GetCustomApiExceptionDescriptionOnUnsuccessfulStatusCode(HttpStatusCode statusCode, TResponse? response);
    }

    public abstract class ApiRequest<TResponse> : IApiRequest<TResponse>
    {
        [JsonIgnore]
        public RestResponse<TResponse>? Response { get; set; }

        [JsonIgnore]
        public string EndpointLogStr => $"({MethodLogStr}){Endpoint}";

        [JsonIgnore]
        public string MethodLogStr => Method.ToString().ToUpperInvariant();

        public abstract string Endpoint { get; }

        public abstract Method Method { get; }

        public object? Body { get; set; }

        public object? Query { get; set; }

        public Dictionary<string, string> Headers { get; set; } = new();

        public bool LogRequest { get; set; } = true;

        /// <summary>
        /// Value used to mask sensitive data in requests/responses.
        /// </summary>
        public static string Mask { get; set; } = "***"; // Update unit test if changing default value

        public ApiRequest(object? body = null)
        {
            Body = body;
        }

        /// <summary>
        /// Optionally add any extra headers.
        /// </summary>
        public virtual Dictionary<string, string>? GetExtraRequestSpecificHeaders() { return null; }


        /// <summary>
        /// Gets the object for request logging.
        /// You should override this method is for example you need to remove any sensitive information from a specific object before logging
        /// </summary>
        /// <returns>The object for request logging.</returns>
        public virtual object GetObjectForRequestLogging()
        {
            if (Body != null && Query != null)
            {
                return new { Body, Query };
            }

            if (Body != null)
                return Body;

            if (Query != null)
                return Query;

            return new object(); // empty
        }

        /// <summary>
        /// Override post processing to perform internal tasks upon a successful response
        /// NOTE: this method is NOT async, do not write blocking code in this method
        /// </summary>
        /// <param name="response">Response.</param>
        public virtual void PostProcessing(TResponse response) { }

        /// <summary>
        /// Gets the object for response logging. Override this method if you need to remove sensitive data before logging
        /// Note, the changes you make on this response object only affect logging.
        /// </summary>
        /// <returns>The object for response logging.</returns>
        public virtual TResponse GetObjectForResponseLogging()
        {
            if (Response == null)
                throw new InvalidOperationException("Cannot get response object for logging without a response");

            return JsonConvert.DeserializeObject<TResponse>(Response.Content!)!;
        }

        /// <summary>
        /// Return an optional more detailed error message instead of the standard HTTP status.
        /// </summary>
        /// <param name="statusCode">HTTP status code.</param>
        /// <param name="response">Types response for the attempted request.</param>
        /// <returns>Null for default behaviour, or optionally a more detailed error message.</returns>
        public virtual string? GetCustomApiExceptionDescriptionOnUnsuccessfulStatusCode(HttpStatusCode statusCode, TResponse? response)
        {
            return null;
        }

        /// <summary>
        /// Some API responses may have a JSON property themselves.
        /// Helper method to find the given property within the JSON, and change it's value to avoid logging anything sensitive.
        /// </summary>
        /// <param name="rawJson">RAW JSON returned as a property on an API response</param>
        /// <param name="jsonKey">The matching property name, not case sensitive. E.g. MyProperty</param>
        /// <param name="maskedValue">Optional mask value, defaulting to the value of <see cref="Mask"/>if null</param>
        /// <returns></returns>
        public static string MaskRawJsonProperty(string rawJson, string jsonKey, string? maskedValue = null)
        {
            maskedValue ??= Mask;

            // If we use regex we can handle scenarios such as this, where IndexOf would fail:
            // "Key":"value"
            // "Key": "value"
            // "Key" : "value"
            // "Key"
            // :
            // "value"

            var pattern = $"(\"{Regex.Escape(jsonKey)}\"\\s*:\\s*\")([^\"]*)(\")";

            return Regex.Replace(
                rawJson,
                pattern,
                $"$1{maskedValue}$3",
                RegexOptions.IgnoreCase
            );
        }

        /// <summary>
        /// Mask a specific query parameter's value within a URL.
        /// </summary>
        /// <param name="url">URL with the sensitive query parameters</param>
        /// <param name="parameterName">The matching query parameter name, not case sensitive. E.g. token</param>
        /// <param name="maskedValue">Optional mask value, defaulting to the value of <see cref="Mask"/>if null</param>
        /// <returns></returns>
        public static string MaskUrlQueryParameter(string url, string? parameterName, string? maskedValue = null)
        {
            maskedValue ??= Mask;

            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(parameterName))
                return url;

            return Regex.Replace(
                url,
                $@"([?&]{Regex.Escape(parameterName)}=)[^&]*",
                $"$1{maskedValue}",
                RegexOptions.IgnoreCase
            );
        }
    }

    public class UriQueryParameterAttribute : Attribute
    {
        public string ParameterName { get; }

        public UriQueryParameterAttribute(string parameterName)
        {
            ParameterName = parameterName;
        }
    }
}
