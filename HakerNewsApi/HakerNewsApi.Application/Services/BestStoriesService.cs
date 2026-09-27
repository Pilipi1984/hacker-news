using HackerNews.Application.Dtos;
using HakerNews.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace HackerNews.Application.Services
{
    public class BestStoriesService : IBestStoriesService
    {
        public Task<IReadOnlyList<StoryDto>> GetBestStoriesAsync(int n, CancellationToken ct)
        {
        }
    }
}
