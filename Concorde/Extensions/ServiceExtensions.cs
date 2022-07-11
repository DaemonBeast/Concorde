using System.Net.WebSockets;
using Concorde.Abstractions;
using Concorde.Abstractions.Client;
using Concorde.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Concorde.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddFactory<TService>(this IServiceCollection services)
        where TService : class
    {
        services.AddTransient<TService>();
        services.AddSingleton<Func<TService>>(provider => provider.GetRequiredService<TService>);
        services.AddSingleton<IFactory<TService>, Factory<TService>>();
        
        return services;
    }
    
    public static IServiceCollection AddFactory<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        services.AddTransient<TService, TImplementation>();
        services.AddSingleton<Func<TService>>(provider => provider.GetRequiredService<TService>);
        services.AddSingleton<IFactory<TService>, Factory<TService>>();
        
        return services;
    }

    public static IServiceCollection AddFactory<TService>(
        this IServiceCollection services,
        Func<IServiceProvider, TService> implementationFactory)
        where TService : class
    {
        services.AddTransient(implementationFactory);
        services.AddSingleton<Func<TService>>(provider => provider.GetRequiredService<TService>);
        services.AddSingleton<IFactory<TService>, Factory<TService>>();

        return services;
    }

    public static IServiceCollection AddConcordeClient<TClient>(this IServiceCollection services)
        where TClient : class, IDiscordClient, IHostedService
    {
        return services.AddConcordeClient<TClient, BaseDiscordRestClient, BaseDiscordSocketClient>();
    }
    
    public static IServiceCollection AddConcordeClient<TClient, TRestClient, TSocketClient>(
        this IServiceCollection services)
        where TClient : class, IDiscordClient, IHostedService
        where TRestClient : class, IDiscordRestClient
        where TSocketClient : class, IDiscordSocketClient
    {
        services.AddHttpClient();

        services.AddFactory<ClientWebSocket>();

        services.AddSingleton<IDiscordRestClient, TRestClient>();
        services.AddSingleton<IDiscordSocketClient, TSocketClient>();

        services.AddHostedService<TClient>();

        return services;
    }
}