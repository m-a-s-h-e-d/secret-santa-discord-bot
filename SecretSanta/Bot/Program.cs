using Bot.Services;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Bot;

public class Program
{
  public static async Task Main(string[] args)
  {
    try
    {
      var builder = Host.CreateApplicationBuilder(args);

      SetupLogger(builder.Configuration);

      SetupServices(builder);

      var host = builder.Build();

      await host.RunAsync();
    }
    catch (Exception ex)
    {
      Log.Fatal(ex, "Host terminated unexpectedly");
    }
    finally
    {
      await Log.CloseAndFlushAsync();
    }
  }

  private static void SetupLogger(IConfiguration config)
  {
    var logConfig = new LoggerConfiguration()
      .MinimumLevel.Information()
      .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
      .WriteTo.Console()
      .CreateLogger();

    Log.Logger = logConfig;
  }

  private static void SetupServices(HostApplicationBuilder builder)
  {
    builder.Services.AddSerilog();

    builder.Services.AddDiscordHost((config, _) =>
    {
      config.SocketConfig = new DiscordSocketConfig
      {
        LogLevel = LogSeverity.Verbose,
        AlwaysDownloadUsers = true,
        MessageCacheSize = 200,
        GatewayIntents = GatewayIntents.All
      };

      config.Token = builder.Configuration["Token"]!;

      config.LogFormat = (message, exception) => $"{message.Source}: {message.Message}";
    });

    builder.Services.AddCommandService((config, _) =>
    {
      config.DefaultRunMode = RunMode.Async;
      config.CaseSensitiveCommands = false;
    });

    builder.Services.AddInteractionService((config, _) =>
    {
      config.LogLevel = LogSeverity.Info;
      config.UseCompiledLambda = true;
    });

    builder.Services.AddHostedService<InteractionHandler>();
    builder.Services.AddHostedService<BotStatusService>();
  }
}