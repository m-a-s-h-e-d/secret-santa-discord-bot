using Discord;
using Discord.WebSocket;
using Schema;

namespace Bot.Utils;

public static class ResponseHandler
{
  private const ushort DefaultImageSize = 64;

  public static Embed? SimpleGuildMessageEmbed(SocketGuild guild, string title, string message)
  {
    var embed = new EmbedBuilder
    {
      Title = title,
      ThumbnailUrl = guild.IconUrl
    };

    embed.WithDescription(message)
      .WithCurrentTimestamp();

    return embed.Build();
  }

  public static Embed? SimpleUserMessageEmbed(SocketUser recipient, string title, string message)
  {
    var image = recipient.GetDisplayAvatarUrl(ImageFormat.Png, DefaultImageSize);
    var embed = new EmbedBuilder
    {
      Title = title,
      ThumbnailUrl = image
    };

    embed.WithDescription(message)
      .WithCurrentTimestamp();

    return embed.Build();
  }

  public static Embed? NotifySecretSantaEmbed(SocketGuild guild,
    SocketUser recipient, decimal budget, string[] preferences)
  {
    var image = recipient.GetDisplayAvatarUrl(ImageFormat.Png, DefaultImageSize);
    var embed = new EmbedBuilder
    {
      Title = $"Secret Santa for [{guild.Name}]",
      ThumbnailUrl= image
    };

    embed.AddField("Recipient", $"{recipient.GlobalName ?? recipient.Username}\n`{recipient.Id}`")
      .WithDescription("You have been assigned a gift recipient for secret santa!")
      .AddField("Budget", $"${budget:#.##}")
      .WithCurrentTimestamp();

    embed.AddPreferences(preferences);
    
    return embed.Build();
  }

  public static Embed? PreferencesListEmbed(SocketUser recipient, IEnumerable<string> preferences)
  {
    var image = recipient.GetDisplayAvatarUrl(ImageFormat.Png, DefaultImageSize);
    var embed = new EmbedBuilder
    {
      Title = $"Preferences List for {recipient.GlobalName ?? recipient.Username}",
      ThumbnailUrl = image
    };

    embed.AddPreferences(preferences);

    embed.WithDescription("Updated your preferences.")
      .WithCurrentTimestamp();

    return embed.Build();
  }

  public static Embed? ParticipantListEmbed(DiscordSocketClient client, SocketGuild guild, string title,
    List<ulong> participants)
  {
    var embed = new EmbedBuilder
    {
      Title = $"{title} for [{guild.Name}]",
      ThumbnailUrl = guild.IconUrl
    };

    foreach (var participantId in participants)
    {
      var user = client.GetUser(participantId);
      embed.AddField(user.GlobalName ?? user.Username,
        $"`{participantId}`");
    }

    embed.WithCurrentTimestamp();

    return embed.Build();
  }

  private static void AddPreferences(this EmbedBuilder embed, IEnumerable<string> preferences)
  {
    var i = 0;
    foreach (var pref in preferences)
    {
      if (pref.Equals("")) continue;
      i++;
      embed.AddField($"Preference #{i}", pref);
    }

    if (i == 0)
    {
      embed.AddField("Preferences", "No preferences specified");
    }
  }
}
