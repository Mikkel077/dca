using Microsoft.EntityFrameworkCore;
using ViaPadel.Core.Domain.Aggregates.DailySchedules;
using ViaPadel.Core.Domain.Aggregates.Players;

namespace ViaPadel.Infrastructure.SqliteDmPersistence;

public class SqliteDmContext : DbContext
{
    public SqliteDmContext()
    {
    }


    public SqliteDmContext(DbContextOptions<SqliteDmContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=viapadel-dm.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>(b =>
        {
            b.ComplexProperty(p => p.Name, nb =>
            {
                nb.Property(n => n.FirstName).HasColumnName("FirstName");
                nb.Property(n => n.LastName).HasColumnName("LastName");
            });
        });
        
        modelBuilder.Entity<Player>(b =>
        {
            b.ComplexProperty(p => p.Picture, pb =>
            {
                pb.Property(pic => pic.Uri)
                    .HasConversion(
                        uri => uri.ToString(),       // Uri -> stored string
                        s   => new Uri(s))           // string -> Uri
                    .HasColumnName("ProfilePictureUri");
            });
        });
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SqliteDmContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<ScheduleId>().HaveConversion<ScheduleIdConverter>();
        configurationBuilder.Properties<PlayerId>().HaveConversion<PlayerIdConverter>();
        configurationBuilder.Properties<BookingId>().HaveConversion<BookingIdConverter>();
        configurationBuilder.Properties<CourtName>().HaveConversion<CourtNameConverter>();
        configurationBuilder.Properties<TimeInterval>().HaveConversion<TimeIntervalConverter>();
    }

    public DbSet<DailySchedule> DailySchedules => Set<DailySchedule>();
    public DbSet<Player> Players => Set<Player>();
    
}