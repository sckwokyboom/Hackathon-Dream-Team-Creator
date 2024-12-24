namespace hrdirector.Entities;

using domain;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class HackathonDbContext(DbContextOptions<HackathonDbContext> options) : DbContext(options)
{
    public DbSet<Hackathon> Entities { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Hackathon>()
            .HasKey(e => e.HackathonId);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var jsonConverter = new ValueConverter<List<Team>?, string>(
            v => JsonSerializer.Serialize(v, options),
            v => JsonSerializer.Deserialize<List<Team>>(v, options) ?? new List<Team>());

        var valueComparerForTeams = new ValueComparer<List<Team>?>(
            (c1, c2) => JsonSerializer.Serialize(c1, options) == JsonSerializer.Serialize(c2, options),
            c => JsonSerializer.Serialize(c, options).GetHashCode(),
            c => JsonSerializer.Deserialize<List<Team>>(JsonSerializer.Serialize(c, options), options)!
        );

        var jsonPreferencesConverter = new ValueConverter<List<Preferences>?, string>(
            v => JsonSerializer.Serialize(v, options),
            v => JsonSerializer.Deserialize<List<Preferences>>(v, options) ??
                 new List<Preferences>());

        var valueComparerForPreferences = new ValueComparer<List<Preferences>?>(
            (c1, c2) => JsonSerializer.Serialize(c1, options) == JsonSerializer.Serialize(c2, options),
            c => JsonSerializer.Serialize(c, options).GetHashCode(),
            c => JsonSerializer.Deserialize<List<Preferences>>(JsonSerializer.Serialize(c, options), options)!
        );

        modelBuilder.Entity<Hackathon>(entity =>
        {
            entity.HasKey(e => e.HackathonId);
            entity.Property(e => e.HackathonId).ValueGeneratedNever();
            entity.Property(e => e.Teams)
                .HasConversion(jsonConverter)
                .Metadata.SetValueComparer(valueComparerForTeams);
            entity.Property(e => e.Preferences)
                .HasConversion(jsonPreferencesConverter)
                .Metadata.SetValueComparer(valueComparerForPreferences);
        });
    }
}