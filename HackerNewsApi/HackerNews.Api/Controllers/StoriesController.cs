using HackerNews.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HackerNews.Api.Controllers
{
    [ApiController]
    [Route("api/stories")]
    public sealed class StoriesController(IBestStoriesService bestStoriesService) : ControllerBase
    {
        private readonly IBestStoriesService _bestStoriesService = bestStoriesService;

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetBestStories(int n, CancellationToken ct)
        {
            if (n <= 0) 
            {
                return BadRequest("Invalid parameter: n must be greater than zero.");
            }

            var stories = await _bestStoriesService.GetBestStoriesAsync(n, ct);
            return Ok(stories);
        }
    }
}
