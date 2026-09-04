using Microsoft.Extensions.DependencyInjection;
using TodoApp.Application.Repositories;
using TodoApp.Infrastructure.Repositories;

namespace TodoApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IProjectRepository, InMemoryProjectRepository>();
        services.AddSingleton<ITodoItemRepository, InMemoryTodoItemRepository>();
        services.AddSingleton<IActivityLogRepository, InMemoryActivityLogRepository>();
        return services;
    }
}
