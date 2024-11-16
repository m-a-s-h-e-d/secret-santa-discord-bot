namespace Schema;

public class Groups(AppDbContext context)
{
  public async Task SaveGroup(ulong id, decimal budget, List<ulong> participants, List<ulong> joinOrder)
  {
    var guild = await context.Groups.FindAsync(id);

    if (guild == null)
    {
      context.Groups.Add(new SecretSantaGroupEntity { Id = id, Budget = budget, Participants = participants, JoinOrder = joinOrder });
    }
    else
    {
      guild.Budget = budget;
      guild.Participants = participants;
      guild.JoinOrder = joinOrder;
    }

    await context.SaveChangesAsync();
  }
}
