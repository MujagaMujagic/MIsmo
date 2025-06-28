using System.ComponentModel.DataAnnotations;

namespace JobNest.Models;

public class CandidateSkill
{
    public int Id { get; set; }


    [MaxLength(50)]
    public string? Level { get; set; } // Beginner, Intermediate, Advanced

    // Navigation properties
    public CandidateProfile CandidateProfile { get; set; } = null!;
}