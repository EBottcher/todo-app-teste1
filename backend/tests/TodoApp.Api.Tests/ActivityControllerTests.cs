using Microsoft.AspNetCore.Mvc;
using TodoApp.Api.Controllers;
using TodoApp.Application.Dtos;

namespace TodoApp.Api.Tests;

public sealed class ActivityControllerTests
{
    [Fact]
    public async Task ListAsync_ReturnsServiceResultsAndForwardsArguments()
    {
        IReadOnlyCollection<ActivityLogDto> activity = [new() { Action = "created" }];
        using var cancellationSource = new CancellationTokenSource();
        var service = new TestActivityService
        {
            ListHandler = (limit, cancellationToken) =>
            {
                Assert.Equal(25, limit);
                Assert.Equal(cancellationSource.Token, cancellationToken);
                return Task.FromResult(activity);
            }
        };

        var result = await new ActivityController(service).ListAsync(25, cancellationSource.Token);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(activity, ok.Value);
    }
}
