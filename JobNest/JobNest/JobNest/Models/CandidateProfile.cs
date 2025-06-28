using System.ComponentModel.DataAnnotations;

namespace JobNest.Models;

public class CandidateProfile
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(500)]
    public string? CVPath { get; set; }

    [MaxLength(1000)]
    public string? Skills { get; set; }

    [MaxLength(2000)]
    public string? Experience { get; set; }

    [MaxLength(100)]
    public string? Location { get; set; }

    [MaxLength(100)]
    public string? PhoneNumber { get; set; }

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
}