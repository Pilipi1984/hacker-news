# Hacker News Best Stories API

RESTful API that retrieves the details of the best `n` stories from the [Hacker News API](https://github.com/HackerNews/API), ordered by score (descending).

## Table of contents

- [Endpoints](#endpoints)
- [How to run](#how-to-run)
- [Configuration](#configuration)
- [Architecture](#architecture)
- [Assumptions](#assumptions)
- [Enhancements and changes with more time](#enhancements-and-changes-with-more-time)

## Endpoints

GET api/v1.0/status

**Responses**

- `200 OK`: API properties.

GET api/v1.0/stories?n={n}

| Parameter | Type | Description |
|-----------|------|-------------|
| `n`       | int  | Number of best stories to return. Must be a positive integer. |

**Responses**

- `200 OK`: array of stories sorted by score, descending.
- `400 Bad Request`: `n` is missing, zero or negative.

## How to run

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Internet access (the app calls `https://hacker-news.firebaseio.com/v0/`)

### Steps

```bash
git clone https://github.com/Pilipi1984/hacker-news.git
cd hacker-news/HackerNewsApi

# Restore, build and run the tests
dotnet restore
dotnet build
dotnet test

# Run the API
dotnet run --project HackerNews.Api
```

The API listens on `http://localhost:5017` (see `Properties/launchSettings.json`).
In the `Development` environment, Swagger UI is available at `http://localhost:5017/swagger`.

Quick check:

```bash
curl "http://localhost:5017/api/v1.0/status"
```

## Configuration

Settings live in `HackerNews.Api/appsettings.json` and can be overridden with environment variables (e.g. `HackerNews__Caching__ItemCacheSeconds=120`).

| Section | Key | Default | Description |
|---------|-----|---------|-------------|
| `HackerNews` | `BaseUrl` | `https://hacker-news.firebaseio.com/v0/` | Upstream API base URL |
| `HackerNews` | `TimeoutSecond` | `10` | HTTP client timeout (seconds) |
| `HackerNews:Caching` | `IdListCacheSeconds` | `60` | Time the list of best story ids is cached |
| `HackerNews:Caching` | `ItemCacheSeconds` | `240` | Time each story is cached |
| `HackerNews:Caching` | `MaxConcurrentUpstreamRequests` | `10` | Max concurrent requests to the Hacker News API |
| `BestStories` | `MaxStories` | `300` | Upper limit applied to `n` |

## Architecture

The solution follows a layered (clean architecture) approach:

```
HackerNewsApi/
├── HackerNews.Api                  # ASP.NET Core Web API (controllers, composition root)
├── HackerNews.Application          # Use cases: BestStoriesService, DTOs, ports (IHackerNewsGateway)
├── HackerNews.Domain               # Entities (Story)
├── HackerNews.Infrastructure       # Hacker News HTTP client, caching decorator, cache warmer
├── HackerNews.Application.Tests    # Unit tests for the service
└── HackerNews.Infrastructure.Tests # Unit tests for the caching gateway
```

Request flow:

1. `StoriesController` validates `n` and calls `IBestStoriesService`.
2. `BestStoriesService` gets the best story ids through `IHackerNewsGateway`, fetches every story in parallel, discards deleted, dead or untitled items, sorts by score (descending) and takes `min(n, MaxStories)`.
3. `IHackerNewsGateway` is implemented by `CachingHackerNewsGateway`, a decorator over `HackerNewsGateway` (plain HTTP client):
   - Ids list and individual stories are cached in memory with independent TTLs.
   - A semaphore limits concurrent upstream requests so a large `n` cannot flood the Hacker News API.
   - A lock on the ids refresh avoids a cache stampede.
4. `HackerNewsGateway` uses `AddStandardResilienceHandler()` (retry, circuit breaker, timeouts) via `Microsoft.Extensions.Http.Resilience`.
5. `BestStoriesCacheWarmer` (a `BackgroundService`) refreshes the ids list before it expires, so requests rarely pay the cost of that upstream call.

## Assumptions

- "Best stories" means the ids returned by the `beststories` endpoint of the Hacker News API. The result is re-sorted by `score` descending, as the requirement asks.
- If `n` is greater than the number of available stories (or than `BestStories:MaxStories`), the API returns as many as possible instead of failing.
- Stories that are deleted, dead, or have no title are excluded, so the response may contain fewer than `n` items.
- If the `url` of a story is missing, `uri` is returned as an empty string.
- If the `by` of a story is missing, `postedBy` is returned as an empty string.
- `time` is exposed as an ISO 8601 date-time converted from the Unix time provided by Hacker News.
- `commentCount` maps to the `descendants` field.
- A story that cannot be retrieved upstream (network error, timeout) is logged and skipped rather than failing the whole request.
- Hacker News data changes slowly enough that a short in-memory cache (1 to 4 minutes) is acceptable.
- A single instance deployment is assumed, so an in-process `IMemoryCache` is enough.
- No authentication is required, as the API only exposes public data.

## Enhancements and changes with more time

**Robustness and correctness**

- Return `ProblemDetails` for validation and upstream errors, and add a global exception handler.
- Do not cache `null` results caused by transient upstream failures.
- Validate the options at startup.
- When upstream API is not available, return '503'.

**Performance and scalability**

- Fetch only as many candidate stories as needed instead of the whole ids list on each call, or pre-compute the sorted top list in the background warmer and serve it from cache.
- Add response caching headers and rate limiting on the public endpoint.

**Observability and operations**

- Structured logging with correlation ids, OpenTelemetry traces and metrics (cache hit ratio, upstream latency).
- Dockerfile and `docker-compose` for a one-command run.

**Testing and quality**

- Integration tests with `WebApplicationFactory` and a mocked upstream (`WireMock.Net` or a fake `HttpMessageHandler`).
- Tests for `HackerNewsGateway` mapping and for `BestStoriesCacheWarmer`.
- Concurrency tests for the throttling and cache-stampede behavior.
- Code coverage gate and static analysis in CI.

**API design**

- Add XML documentation and response examples to the OpenAPI spec.