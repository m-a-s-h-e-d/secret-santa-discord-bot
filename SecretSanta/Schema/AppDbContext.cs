using Microsoft.EntityFrameworkCore;

namespace Schema;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<SecretSantaGroupEntity> Groups { get; set; }
  public DbSet<PreferenceEntity> Preferences { get; set; }
}

public class SecretSantaGroupEntity
{
  public ulong Id { get; set; }
  public decimal Budget { get; set; }
  public ulong[] Participants { get; set; } = [];
  public ulong[] JoinOrder { get; set; } = [];
}

public class PreferenceEntity
{
  public ulong Id { get; set; }
  public string[] Preferences { get; set; } = [];
}