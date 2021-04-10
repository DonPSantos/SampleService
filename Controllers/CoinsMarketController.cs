using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SampleService.Services;

namespace SampleService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CoinsMarketController : ControllerBase
    {
        private readonly ILogger<CoinsMarketController> _logger;
        private readonly IApiClient _apiClient;

        public CoinsMarketController(
            ILogger<CoinsMarketController> logger,
            IApiClient apiClient)
        {
            _logger = logger;
            _apiClient = apiClient;
        }

        [HttpGet]
        [Route("{currency}")]
        public IActionResult Get(string currency)
        {
            var result = _apiClient.ConnectToApi(currency);

            return Ok(result);
        }
    }
}