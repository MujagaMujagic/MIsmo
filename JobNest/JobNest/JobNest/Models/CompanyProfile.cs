using System.ComponentModel.DataAnnotations;

namespace JobNest.Models;

public class CompanyProfile
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required]
    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? Website { get; set; }

    [MaxLength(100)]
    public string? Industry { get; set; }

    [MaxLength(100)]
    public string? Location { get; set; }

    [MaxLength(100)]
    public string? PhoneNumber { get; set; }

    [MaxLength(100)]
    public string? ContactEmail { get; set; }

    [MaxLength(50)]
    public string? ContactPhone { get; set; }

    [MaxLength(50)]
    public string? EmployeeCount { get; set; }

    public decimal? Rating { get; set; }

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<JobPost> JobPosts { get; set; } = new List<JobPost>();
}