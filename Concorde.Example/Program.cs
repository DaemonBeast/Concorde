using Concorde.Client;
using Concorde.Extensions;
using Concorde.Utilities;
using DotNetEnv;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Concorde.Example;

public static class Program
{
    public static int Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        Env.NoClobber().Load();

        try
        {
            Log.Information("Starting Concorde v{Version}", DotnetUtilities.Version);
            CreateHostBuilder(args).Build().Run();
            return 0;
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Concorde terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static IConfiguration CreateConfiguration(string[] args)
    {
        var configurationBuilder = new ConfigurationBuilder();
        
        configurationBuilder.AddEnvironmentVariables("CONCORDE_");
        configurationBuilder.AddCommandLine(args);

        return configurationBuilder.Build();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
#if DEBUG
            .UseEnvironment("Development")
#else
            .UseEnvironment("Production")
#endif
            .ConfigureAppConfiguration(builder =>
            {
                builder.AddConfiguration(CreateConfiguration(args));
            })
            .ConfigureServices((host, services) =>
            {
                services.Configure<HostOptions>(hostOptions =>
                {
                    hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.StopHost;
                });
                
                services.AddConcordeClient<ExampleDiscordClient>();
            })
            .UseSerilog((context, config) =>
            {
                if (args.Contains("--verbose"))
                {
                    config
                        .MinimumLevel.Is(LogEventLevel.Verbose)
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Verbose)
                        .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Verbose);
                }
                else if (args.Contains("--errors-only"))
                {
                    config
                        .MinimumLevel.Is(LogEventLevel.Error)
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                        .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Error);
                }
                else
                {
                    config
#if DEBUG
                        .MinimumLevel.Is(LogEventLevel.Debug)
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
                        .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning);
#else
                        .MinimumLevel.Is(LogEventLevel.Information)
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                        .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning);
#endif
                }

                config
                    .Enrich.FromLogContext()
                    .WriteTo.Console();
            });
    }
}