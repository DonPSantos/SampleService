using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using SampleService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SampleService.HealthChecks
{
    public class CoinsInfoHealthCheck : IHealthCheck
    {
        private readonly ServiceSettings _settings;

        public CoinsInfoHealthCheck(IOptions<ServiceSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            Ping ping = new();
            var reply = await ping.SendPingAsync(_settings.CoinsPriceUrl);

            if (reply.Status != IPStatus.Success)
                return HealthCheckResult.Unhealthy();

            return HealthCheckResult.Healthy();
        }
    }
}