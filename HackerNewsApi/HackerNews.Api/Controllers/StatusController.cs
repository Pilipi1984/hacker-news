using Asp.Versioning;
using HackerNews.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace HackerNews.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/status")]
    [ProducesResponseType(typeof(StatusResponse), StatusCodes.Status200OK)]
    public class StatusController : ControllerBase
    {
        private static readonly DateTimeOffset StartedAt = DateTimeOffset.UtcNow;

        /// <summary>
        /// Gets the status of the API.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            var apiVersion = HttpContext.RequestedApiVersion;
            var response = new StatusResponse
            {
                Status = "OK",
                Version = apiVersion?.ToString() ?? "unknown",
                Environment = HttpContext.RequestServices
                    .GetRequiredService<IWebHostEnvironment>().EnvironmentName,
                UtcTimestamp = DateTime.UtcNow,
                Uptime = DateTime.UtcNow - StartedAt
            };

            return Ok(response);
        }
    }
}
