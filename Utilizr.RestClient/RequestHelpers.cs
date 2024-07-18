using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;

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
    }

    public interface IApiRequest<TResponse> : IApiRequest
    {
        /// <summary>
        /// Gets the object for request logging.
        /// You should override this method is for example you need to remove any sensitive information from a specific object before logging
        /// </summary>
        /// <returns>The object for request logging.</returns>
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
        TResponse GetObjectForResponseLogging(TResponse response);
    }

    public abstract class ApiRequest<TResponse> : IApiRequest<TResponse>
    {
        [JsonIgnore]
        public string EndpointLogStr => $"({MethodLogStr}){Endpoint}";
        
        [JsonIgnore]
        public string MethodLogStr => Method.ToString().ToUpperInvariant();

        public abstract string Endpoint { get; }

        public abstract Method Method { get; }

        public object? Body { get; set; }

        public object? Query { get; set; }

        public Dictionary<string, string> Headers { get; set; } = new();

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
        public virtual TResponse GetObjectForResponseLogging(TResponse response)
        {
            return response;
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
