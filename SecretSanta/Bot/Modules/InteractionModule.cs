using Bot.Models;
using Discord;
using Discord.Interactions;

namespace Bot.Modules;

public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
{
  private static readonly Dictionary<ulong, SecretSantaGroup> Groups = new();
  private static readonly Dictionary<ulong, string[]> GiftPreferences = new();

  [SlashCommand("join", "Join the server's secret santa list")]
  public async Task Join()
  {
    var group = GetGroup(Context.Guild.Id);
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
    var group = GetGroup(Context.Guild.Id);
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

  [RequireUserPermission(GuildPermission.Administrator)]
  [SlashCommand("assign", "Shuffle and assign secret santas to gift recipients, should only be used once")]
  public async Task Assign()
  {
    var group = GetGroup(Context.Guild.Id);
    var participants = group.Participants();

    if (!group.ValidSize())
    {
      await RespondAsync("Not enough participants to start.");
    }
    else
    {
      await RespondAsync("Shuffling participants...");
      group.Shuffle();
      group.Assign();
      foreach (var secretSantaId in participants)
      {
        var recipientId = (ulong)group.GetRecipient(secretSantaId)!;
        var secretSanta = Context.Client.GetUser(secretSantaId);
        var recipient = Context.Client.GetUser(recipientId);

        await secretSanta.SendMessageAsync(
          $"You are the secret santa of `{recipient.GlobalName ?? recipient.Username}` (User ID: {recipientId})" +
          $"\nTheir gift preferences are: {string.Join(", ", GiftPreferences[recipientId])}" +
          $"\nThe budget for this secret santa is ${group.Budget}");
      }
      await FollowupAsync("Sent direct messages to all secret santas with their gift recipients.");
    }
  }

  [SlashCommand("remind-me", "Remind the command issuer their gift recipient in a direct message")]
  public async Task RemindMe()
  {
    var group = GetGroup(Context.Guild.Id);
    var recipientId = group.GetRecipient(Context.User.Id);
    
    if (recipientId == null)
    {
      await RespondAsync("You have not been assigned a recipient yet.", ephemeral: true);
    }
    else
    {
      var recipient = Context.Client.GetUser((ulong)recipientId);

      await Context.User.SendMessageAsync(
        $"You are the secret santa of `{recipient.GlobalName ?? recipient.Username}` (User ID: {recipientId})" + 
        $"\nTheir gift preferences are: {string.Join(", ", GiftPreferences[(ulong)recipientId])}" +
        $"\nThe budget for this secret santa is ${group.Budget}");
      await RespondAsync("Sent you a direct message with your gift recipient.", ephemeral: true);
    }
  }

  [SlashCommand("set-budget", "Set the server's budget for gifts")]
  public async Task SetBudget(decimal budget)
  {
    var group = GetGroup(Context.Guild.Id);

    if (budget < 0)
    {
      await RespondAsync("You must set the budget to a positive decimal number");
    }
    else
    {
      group.Budget = budget;
      await RespondAsync($"The server's gift budget has been set to ${group.Budget}");
    }
  }

  [SlashCommand("get-budget", "Get the server's budget for gifts")]
  public async Task GetBudget()
  {
    var group = GetGroup(Context.Guild.Id);

    await RespondAsync($"The server's gift budget is ${group.Budget}");
  }

  [SlashCommand("set-preferences", "Set your global gift preferences, shared between all servers")]
  public async Task SetPreferences(string? firstChoice = null, string? secondChoice = null, string? thirdChoice = null)
  {
    var userId = Context.User.Id;

    if (firstChoice == null && secondChoice == null && thirdChoice == null)
    {
      await RespondAsync("No preferences were entered.", ephemeral: true);
    }
    else
    {
      var preferences = GetPreferences(userId);

      if (firstChoice != null)
      {
        preferences[0] = firstChoice;
      }
      if (secondChoice != null)
      {
        preferences[1] = secondChoice;
      }
      if (thirdChoice != null)
      {
        preferences[2] = thirdChoice;
      }

      await RespondAsync($"Updated your preferences: {string.Join(", ", GiftPreferences[userId])}");
    }
  }

  [SlashCommand("list", "See the current participants in the secret santa list (by join order)")]
  public async Task List()
  {
    var group = GetGroup(Context.Guild.Id);

    if (group.List().Count == 0)
    {
      await RespondAsync("The current secret santa list is empty.");
    }
    else
    {
      await RespondAsync($"Participants: [{string.Join(", ", group.List().ToArray())}]");
    }
  }

  [RequireTeam]
  [SlashCommand("peek", "Peek at the current secret santa list in the server (Debug Only)")]
  public async Task Peek()
  {
    var group = GetGroup(Context.Guild.Id);

    if (group.Participants().Count == 0)
    {
      await RespondAsync("The current secret santa list is empty.", ephemeral: true);
    }
    else
    {
      await RespondAsync($"Current group list: [{string.Join(", ", group.Participants().ToArray())}]", ephemeral: true);
    }
  }

  private static string[] GetPreferences(ulong userId)
  {
    string[] preferences;

    try
    {
      preferences = GiftPreferences[userId];
    }
    catch (KeyNotFoundException ex)
    {
      GiftPreferences[userId] = ["", "", ""];
      preferences = GiftPreferences[userId];
    }

    return preferences;
  }

  private static SecretSantaGroup GetGroup(ulong guildId)
  {
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

    return group; 
  }
}
