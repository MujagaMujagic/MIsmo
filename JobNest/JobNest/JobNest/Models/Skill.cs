using System.ComponentModel.DataAnnotations;

namespace JobNest.Models;

public class Skill
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Category { get; set; }
    
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<CandidateSkill> CandidateSkills { get; set; } = new List<CandidateSkill>();
}