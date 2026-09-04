using Microsoft.Extensions.DependencyInjection;
using TodoApp.Application.Services;

namespace TodoApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITodoService, TodoService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IActivityService, ActivityService>();
        return services;
    }
}
