using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System.Net;
using System.Reflection;
using System.Web;
using Utilizr.Logging;

namespace Utilizr.Rest.Client
{
    public delegate void PostRequestHook<TRequest, TResponse>(TRequest request, TResponse response) where TRequest : ApiRequest<TResponse>;

    public abstract class AbstractRestClient : RestClient
    {
        public delegate void ResponseReceivedDelegate(object? response, Type requestType);
        public delegate void AuthenticationInvalidDelegate(HttpStatusCode statusCode, string? message);

        public event ResponseReceivedDelegate? ResponseReceived;
        public event AuthenticationInvalidDelegate? AuthenticationError;

        public bool LogRequests { get; set; } = true;

        protected const string LOG_CAT = "API-CLIENT";
        readonly string _serviceUrl;
        protected readonly List<HookHolder> _hooks;

        public AbstractRestClient(string serviceUrl)
            : base(serviceUrl, configureSerialization:sOptions => sOptions.UseNewtonsoftJson())
        {
            _serviceUrl = serviceUrl;
            _hooks = new List<HookHolder>();
        }

        public virtual Task<T> ExecuteAsync<T>(IApiRequest<T> apiRequest)
        {
            return Task.Run(() => Execute(apiRequest));
        }

        public virtual T Execute<T>(IApiRequest<T> apiRequest)
        {
            var headers = GetHeaders(apiRequest);
            var extraHeaders = apiRequest.GetExtraRequestSpecificHeaders();
            if (extraHeaders != null)
            {
                foreach (var extraHeader in extraHeaders)
                {
                    // Make sure extra headers overwrite any default headers
                    headers[extraHeader.Key] = extraHeader.Value;
                }
            }

            var request = new RestRequest(apiRequest.Endpoint, apiRequest.Method);
            foreach (var header in headers)
            {
                request.AddHeader(header.Key, header.Value);
            }

            if (apiRequest.Body != null)
            {
                request.AddBody(JsonConvert.SerializeObject(apiRequest.Body), ContentType.Json);
            }

            if (apiRequest.Query != null)
            {
                var qs = DtoToQueryString(apiRequest.Query);
                request.Resource += $"?{qs}";
            }

            try
            {
                LogRequest(apiRequest, headers);
                var response = this.Execute<T>(request);
                LogResponse(apiRequest, response);

                if (string.IsNullOrEmpty(response.Content))
                {
                    // todo: no internet connection check / custom error thrown so we can show a nicer error message

                    // Only throw for no content, otherwise stuff like HTTP 400 will cause it to throw
                    // when in reality we want to show a custom api error message of something like
                    // credentials don't match, or weak password detected on signup, etc
                    response.ThrowIfError();
                }

                if (!response.IsSuccessStatusCode)
                {
                    var description = response.StatusDescription;

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                        OnAuthenticationError(response.StatusCode, description);

                    var detailedErrorDescription = apiRequest.GetCustomApiExceptionDescriptionOnUnsuccessfulStatusCode(response.StatusCode, response.Data);
                    if (!string.IsNullOrEmpty(detailedErrorDescription))
                        description = detailedErrorDescription;

                    throw new ApiException((int)response.StatusCode, description, response.Content);
                }

                //var responseData = JsonConvert.DeserializeObject<T>(response.Content!);
                if (response.Data == null)
                    throw new Exception($"Failed to deserialise response on {apiRequest.Endpoint}", response.ErrorException);

                apiRequest.PostProcessing(response.Data);

                NotifyRequestCompleted<T>(apiRequest, response.Data);
                return response.Data;
            }
            catch (Exception e)
            {
                Log.Exception(LOG_CAT, e);
                throw;
            }
        }

        void LogRequest<T>(IApiRequest<T> apiRequest, Dictionary<string, string> headers)
        {
            if (!LogRequests || !apiRequest.LogRequest)
                return;

            var debugDict = new Dictionary<string, object>();
            debugDict["Endpoint"] = apiRequest.Endpoint;
            debugDict["Headers"] = headers;
            debugDict["RequestParams"] = apiRequest.GetObjectForRequestLogging();
            debugDict["FullUrl"] = $"({apiRequest.MethodLogStr}){_serviceUrl}/{apiRequest.Endpoint}";

            Log.Objects(LoggingLevel.INFO, LOG_CAT, new[] { debugDict }, $"Executing api request {apiRequest.EndpointLogStr}");
        }

        void LogResponse<T>(IApiRequest<T> apiRequest, RestResponse response)
        {
#if DEBUG
            Log.Info(LOG_CAT, $"Received response (RAW) from api request {apiRequest.EndpointLogStr} : [{response.Content}]");
#endif

            if (!LogRequests || !apiRequest.LogRequest)
                return;

            var debugDict = new Dictionary<string, object?>();
            debugDict["Endpoint"] = apiRequest.Endpoint;
            debugDict["Headers"] = response.ContentHeaders;
            debugDict["Error"] = response.ErrorMessage ?? response.ErrorException?.Message;
            try
            {
                debugDict["ResponseData"] = apiRequest.GetObjectForResponseLogging(JsonConvert.DeserializeObject<T>(response.Content!)!)!;
            }
            catch (Exception)
            {
                debugDict["ResponseData"] = response!.Content;
            }

            Log.Objects(LoggingLevel.INFO, LOG_CAT, new[] { debugDict }, $"Received response from api request {apiRequest.EndpointLogStr}");
        }

        public static string DtoToQueryString<T>(T obj)
        {
            var nvc = HttpUtility.ParseQueryString(string.Empty);
            var flags = BindingFlags.Public |
                        BindingFlags.Instance |
                        BindingFlags.NonPublic |
                        BindingFlags.DeclaredOnly |
                        BindingFlags.GetProperty;
            var propertyInfos = obj!.GetType().GetProperties(flags);
            foreach (var propertyInfo in propertyInfos)
            {
                if (propertyInfo.GetValue(obj) != null)
                {
                    var attribute = propertyInfo.GetCustomAttribute<UriQueryParameterAttribute>();
                    if (attribute != null)
                    {
                        var queryParamName = attribute.ParameterName;
                        var queryParamValue = propertyInfo.GetValue(obj)?.ToString() ?? string.Empty;

                        nvc.Add(
                            Uri.EscapeDataString(queryParamName),
                            Uri.EscapeDataString(queryParamValue)
                        );
                    }
                }
            }

            return nvc.ToString()!;
        }

        public Dictionary<string, string> GetHeaders(IApiRequest requestObj)
        {
            var postData = requestObj.Body != null
                ? JsonConvert.SerializeObject(requestObj.Body)
                : string.Empty;

            var headers = new Dictionary<string, string>();

            foreach (var key in requestObj.Headers.Keys)
            {
                headers[key] = requestObj.Headers[key];
            }

            ApplyClientSpecificHeaders(headers, requestObj, postData);
            return headers;
        }

        /// <summary>
        /// Optional override to add headers for a specific rest client. 
        /// </summary>
        protected virtual void ApplyClientSpecificHeaders(Dictionary<string, string> headers, IApiRequest requestObj, string postData) { }

        /// <summary>
        /// Hook will fire before the <see cref="ResponseRecieved"/> fires on the same response.
        /// </summary>
        public void AddCustomRequestCompleteHook<TRequest, TResponse>(PostRequestHook<TRequest, TResponse> hook)
            where TRequest : ApiRequest<TResponse>
        {
            var hookHolder = new HookHolder()
            {
                HookDelegate = hook,
                RequestType = typeof(TRequest)
            };
            _hooks.Add(hookHolder);
        }

        protected virtual void NotifyRequestCompleted<TResponse>(IApiRequest<TResponse> request, TResponse response)
        {
            foreach (var hookHolder in _hooks)
            {
                try
                {
                    if (hookHolder.RequestType == request.GetType())
                    {
                        ((Delegate)hookHolder.HookDelegate).DynamicInvoke(request, response);
                    }
                }
                catch (Exception ex)
                {
                    Log.Exception("REST_HOOKS", ex);
                }
            }

            OnResponseReceived(response, request.GetType());
        }

        protected virtual void OnResponseReceived(object? response, Type requestType)
        {
            ResponseReceived?.Invoke(response, requestType);
        }

        protected virtual void OnAuthenticationError(HttpStatusCode statusCode, string? message)
        {
            AuthenticationError?.Invoke(statusCode, message);
        }

    }

    public class HookHolder
    {
        public object HookDelegate { get; set; }
        public Type RequestType { get; set; }
    }
}