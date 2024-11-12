using Bot.Models;
using Discord;
using Discord.Interactions;

namespace Bot.Modules;

public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
{
  private static readonly Dictionary<ulong, SecretSantaGroup> Groups = new();

  [SlashCommand("echo", "Echo an input")]
  public async Task Echo(string input)
  {
    await RespondAsync(input);
  }

  [SlashCommand("join", "Join the server's secret santa list")]
  public async Task Join()
  {
    var guildId = Context.Guild.Id;
    SecretSantaGroup group;
    try
    {
      group = Groups[guildId];
    }
    catch (KeyNotFoundException ex)
    {
      Groups[guildId] = new SecretSantaGroup();
      group = Groups[guildId];
    }
    var userId = Context.User.Id;
    if (group.Join(userId))
    {
      await RespondAsync("Added you to the server's secret santa list.", ephemeral: true);
    }
    else
    {
      await RespondAsync("You are already in the server's secret santa list.", ephemeral: true);
    }
  }

  [SlashCommand("leave", "Leave the server's secret santa list")]
  public async Task Leave()
  {
    var guildId = Context.Guild.Id;
    SecretSantaGroup group;
    try
    {
      group = Groups[guildId];
    }
    catch (KeyNotFoundException ex)
    {
      Groups[guildId] = new SecretSantaGroup();
      group = Groups[guildId];
    }
    var userId = Context.User.Id;
    if (group.Leave(userId))
    {
      await RespondAsync("Removed you to the server's secret santa list.", ephemeral: true);
    }
    else
    {
      await RespondAsync("You are not in the server's secret santa list.", ephemeral: true);
    }
  }

  [SlashCommand("assign", "Shuffle and assign secret santas to gift recipients")]
  public async Task Assign()
  {
    var guildId = Context.Guild.Id;
    SecretSantaGroup group;
    try
    {
      group = Groups[guildId];
    }
    catch (KeyNotFoundException ex)
    {
      Groups[guildId] = new SecretSantaGroup();
      group = Groups[guildId];
    }
    var participants = group.Participants();
    if (participants.Count < 2)
    {
      await RespondAsync("Not enough participants to start.");
    }
    else
    {
      await RespondAsync("Sending DMs to all participants with their recipients.");

      group.Shuffle();

      for (var i = 0; i < participants.Count; i++)
      {
        var secretSantaId = participants[i];
        var recipientId = participants[(i + 1) % participants.Count];
        var secretSanta = Context.Client.GetUser(secretSantaId);
        var recipient = Context.Client.GetUser(recipientId);
        await secretSanta.SendMessageAsync(
          $"You are the secret santa of `{recipient.GlobalName}` (User ID: {recipientId})");
      }
    }
  }

  [SlashCommand("list", "See the current participants in the secret santa list (by join order)")]
  public async Task List()
  {
    var guildId = Context.Guild.Id;
    SecretSantaGroup group;
    try
    {
      group = Groups[guildId];
    }
    catch (KeyNotFoundException ex)
    {
      Groups[guildId] = new SecretSantaGroup();
      group = Groups[guildId];
    }
    if (group.List().Count == 0)
    {
      await RespondAsync("The current secret santa list is empty.");
    }
    else
    {
      await RespondAsync($"Participants: [{string.Join(",", group.List().ToArray())}]");
    }
  }

  [RequireTeam]
  [SlashCommand("peek", "Peek at the current secret santa list in the server (Debug Only)")]
  public async Task Peek()
  {
    var guildId = Context.Guild.Id;
    SecretSantaGroup group;
    try
    {
      group = Groups[guildId];
    }
    catch (KeyNotFoundException ex)
    {
      Groups[guildId] = new SecretSantaGroup();
      group = Groups[guildId];
    }
    if (group.Participants().Count == 0)
    {
      await RespondAsync("The current secret santa list is empty.", ephemeral: true);
    }
    else
    {
      await RespondAsync($"Current group list: [{string.Join(",", group.Participants().ToArray())}]", ephemeral: true);
    }
  }
}
