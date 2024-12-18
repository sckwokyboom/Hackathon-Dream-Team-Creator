namespace hrdirector.Entities;

using domain;
using System.ComponentModel.DataAnnotations;

public record Hackathon
{
    [Key] public int HackathonId { get; set; }
    public decimal Harmony { get; set; }
    public List<Team>? Teams { get; set; } = [];
    public List<Preferences>? Preferences { get; set; } = [];
}