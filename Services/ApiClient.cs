using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using RestSharp;
using SampleService.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;

namespace SampleService.Services
{
    public class ApiClient : IApiClient
    {
        private readonly ServiceSettings _settings;
        private readonly ILogger<ApiClient> _logger;

        private static readonly List<HttpStatusCode> invalidStatusCode = new List<HttpStatusCode> {
            HttpStatusCode.BadRequest,
            HttpStatusCode.BadGateway,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.RequestTimeout,
            HttpStatusCode.Forbidden,
            HttpStatusCode.GatewayTimeout
        };

        public ApiClient(
            ILogger<ApiClient> logger,
            IOptions<ServiceSettings> settings)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public CoinsInfo ConnectToApi(string currency)
        {
            var retryPolicy = Policy
                .HandleResult<IRestResponse>(resp => invalidStatusCode.Contains(resp.StatusCode))
                .WaitAndRetry(10, i => TimeSpan.FromSeconds(Math.Pow(2, i)), (result, TimeSpan, currentRetryCount, context) =>
                {
                    _logger.LogError($"Request has failed with a {result.Result.StatusCode}. Waiting {TimeSpan} before next retry. This is the {currentRetryCount} retry ");
                });

            var client = new RestClient($"{_settings.CoinsPriceUrl}/ticker");

            var request = new RestRequest(Method.GET);
            request.RequestFormat = DataFormat.Json;

            request.AddParameter("key", _settings.ApiKey, ParameterType.GetOrPost);
            request.AddParameter("label", "ethbtc-ltcbtc-btcbtc", ParameterType.GetOrPost);
            request.AddParameter("fiat", currency, ParameterType.GetOrPost);

            var policyResponse = retryPolicy.ExecuteAndCapture(() =>
            {
                return client.Get(request);
            });

            if (policyResponse.Result != null)
            {
                return JsonSerializer.Deserialize<CoinsInfo>(policyResponse.Result.Content);
            }
            else
            {
                return null;
            }
        }

        public record Market(string Label, string Name, double Price);
        public record CoinsInfo(Market[] Markets);
    }
}