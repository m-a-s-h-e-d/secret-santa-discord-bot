namespace Schema;

public class Preferences(AppDbContext context)
{
  public async Task SavePreference(ulong id, string[] preferences)
  {
    var user = await context.Preferences.FindAsync(id);

    if (user == null)
    {
      context.Preferences.Add(new PreferenceEntity { Id = id, Preferences = preferences });
    }
    else
    {
      user.Preferences = preferences;
    }

    await context.SaveChangesAsync();
  }
}
