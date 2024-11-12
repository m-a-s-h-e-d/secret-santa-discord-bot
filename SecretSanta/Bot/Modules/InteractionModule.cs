using Discord;
using Discord.Interactions;

namespace Client.Modules;

public class InteractionModule : InteractionModuleBase<ShardedInteractionContext>
{
  [SlashCommand("echo", "Echo an input")]
  public async Task Echo(string input)
  {
    await RespondAsync(input);
  }

  [RequireOwner]
  [SlashCommand("test", "Echo an input (Must match UID)")]
  public async Task Test(string input)
  {
    await RespondAsync(input);
  }

  [SlashCommand("multi", "Test multiple parameters")]
  public async Task Test(string input, int number, Optional<bool> flag)
  {
    await RespondAsync($"{input} : {number} : {flag}");
  }
}
