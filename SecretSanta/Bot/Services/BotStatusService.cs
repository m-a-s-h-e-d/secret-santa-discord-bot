using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Bot.Services;

internal class BotStatusService(DiscordShardedClient client, ILogger<BotStatusService> logger) : DiscordShardedClientService(client, logger)
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    await Client.WaitForReadyAsync(stoppingToken);

    await Client.SetCustomStatusAsync("Assigning secret santas!");
  }
}
