using Bot.Core.Models;
using Bot.Utils;
using Discord;
using Discord.Interactions;
using Schema;

namespace Bot.Modules;

public class InteractionModule(AppDbContext dbContext, Groups groups, Preferences giftPreferences) : InteractionModuleBase<SocketInteractionContext>
{
  private static Dictionary<ulong, SecretSantaGroup> _groups = new();
  private static Dictionary<ulong, string[]> _giftPreferences = new();

  [SlashCommand("join", "Join the server's secret santa list")]
  public async Task Join()
  {
    var group = GetGroup(Context.Guild.Id);
    var userId = Context.User.Id;

    if (group.Join(userId))
    {
      await groups.SaveGroup(Context.Guild.Id, group.Budget, group.Participants(), group.List());
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
      await groups.SaveGroup(Context.Guild.Id, group.Budget, group.Participants(), group.List());
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
    var guild = Context.Guild;
    var group = GetGroup(guild.Id);
    var participants = group.Participants();

    if (!group.ValidSize())
    {
      await RespondAsync("Not enough participants to start.");
    }
    else
    {
      await RespondAsync("Shuffling participants...");
      group.Shuffle();
      await groups.SaveGroup(Context.Guild.Id, group.Budget, group.Participants(), group.List());
      group.Assign();
      foreach (var secretSantaId in participants)
      {
        var recipientId = (ulong)group.GetRecipient(secretSantaId)!;
        var secretSanta = Context.Client.GetUser(secretSantaId);
        var recipient = Context.Client.GetUser(recipientId);

        await secretSanta.SendMessageAsync(
          embed: ResponseHandler.NotifySecretSantaEmbed(
            guild, recipient, group.Budget, GetPreferences(recipientId)
          )
        );
      }
      await FollowupAsync("Sent direct messages to all secret santas with their gift recipients.");
    }
  }

  [SlashCommand("remind-me", "Remind the command issuer their gift recipient in a direct message")]
  public async Task RemindMe()
  {
    var guild = Context.Guild;
    var group = GetGroup(guild.Id);
    var recipientId = group.GetRecipient(Context.User.Id);
    
    if (recipientId == null)
    {
      await RespondAsync("You have not been assigned a recipient yet.", ephemeral: true);
    }
    else
    {
      var recipient = Context.Client.GetUser((ulong)recipientId);

      await Context.User.SendMessageAsync(
        embed: ResponseHandler.NotifySecretSantaEmbed(
          guild, recipient, group.Budget, GetPreferences((ulong)recipientId)
        )
      );
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
      await groups.SaveGroup(Context.Guild.Id, group.Budget, group.Participants(), group.List());
      await RespondAsync($"The server's gift budget has been set to ${group.Budget:#.##}");
    }
  }

  [SlashCommand("get-budget", "Get the server's budget for gifts")]
  public async Task GetBudget()
  {
    var group = GetGroup(Context.Guild.Id);

    await RespondAsync($"The server's gift budget is ${group.Budget:#.##}");
  }

  [SlashCommand("set-preferences", "Set your global gift preferences, shared between all servers (Links OK)")]
  public async Task SetPreferences(
    [Summary("First", "Your top preference for gifts, input CLEAR to clear this preference")]
    string? firstChoice = null,
    [Summary("Second", "Your second top preference for gifts, input CLEAR to clear this preference")]
    string? secondChoice = null,
    [Summary("Third", "Your third top preference for gifts, input CLEAR to clear this preference")]
    string? thirdChoice = null)
  {
    var userId = Context.User.Id;
    var preferences = GetPreferences(userId);
    var newPreferences = new List<string>();
    if (firstChoice == null && secondChoice == null && thirdChoice == null)
    {
      await RespondAsync("No preferences were entered.", ephemeral: true);
    }
    else
    {
      var i = 0;
      foreach (var choice in new List<string?> { firstChoice, secondChoice, thirdChoice })
      {
        switch (choice)
        {
          case null:
            newPreferences.Add(preferences[i++]);
            break;
          case "CLEAR":
            break;
          default:
            newPreferences.Add(choice);
            break;
        }
      }

      await giftPreferences.SavePreference(userId, newPreferences.ToArray());
      await RespondAsync(embed: ResponseHandler.PreferencesList(Context.User, newPreferences));
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
      await RespondAsync(
        embed: ResponseHandler.ParticipantList(
          Context.Client, Context.Guild, "Participant List (By Join Order)", group.List()
        )
      );
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

      await RespondAsync(
        embed: ResponseHandler.ParticipantList(
          Context.Client, Context.Guild, "Participant List (In Shuffled Order)", group.Participants()
        ),
        ephemeral: true
      );
    }
  }

  private string[] GetPreferences(ulong userId)
  {
    string[] preferences;

    if (_giftPreferences.Count <= 0)
    {
      _giftPreferences = dbContext.Preferences.ToDictionary(c => c.Id, c => c.Preferences);
    }

    try
    {
      preferences = _giftPreferences[userId];
    }
    catch (KeyNotFoundException ex)
    {
      _giftPreferences[userId] = ["", "", ""];
      preferences = _giftPreferences[userId];
    }

    return preferences;
  }

  private SecretSantaGroup GetGroup(ulong guildId)
  {
    SecretSantaGroup group;

    if (_groups.Count <= 0)
    {
      _groups = dbContext.Groups.ToDictionary(c => c.Id,
        c => new SecretSantaGroup(c.Budget, c.Participants, c.JoinOrder));
    }

    try
    {
      group = _groups[guildId];
    }
    catch (KeyNotFoundException ex)
    {
      _groups[guildId] = new SecretSantaGroup();
      group = _groups[guildId];
    }

    return group;
  }
}
