using System.ComponentModel.DataAnnotations;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace Schema;

public class AppDbContext : DbContext
{
  public DbSet<SecretSantaGroupEntity> Groups { get; set; }
  public DbSet<PreferenceEntity> Preferences { get; set; }
  public string DbPath { get; }

  public AppDbContext()
  {
    // The environment variable paths probably don't work on Docker containers
    // const Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData; 
    // var DbPath = Path.GetFullPath(Environment.GetFolderPath(folder));
    // DbPath = Path.Join(path, "secretsanta.db");
    DbPath = "secretsanta.db";
  }

  // The following configures EF to create a Sqlite database file in the
  // special "local" folder for your platform.
  protected override void OnConfiguring(DbContextOptionsBuilder options)
    => options.UseSqlite($"Data Source={DbPath}");
}

public class SecretSantaGroupEntity
{
  [Key]
  public ulong Id { get; set; }
  public decimal Budget { get; set; }
  public List<ulong> Participants { get; set; } = [];
  public List<ulong> JoinOrder { get; set; } = [];
}

public class PreferenceEntity
{
  [Key]
  public ulong Id { get; set; }
  public string[] Preferences { get; set; } = [];
}