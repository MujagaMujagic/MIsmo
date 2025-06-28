using System.ComponentModel.DataAnnotations;

namespace JobNest.Models;

public class JobApplication
{
    public int Id { get; set; }

    public int JobPostId { get; set; }

    public int CandidateId { get; set; }

    [MaxLength(200)]
    public string CandidateName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string CandidateEmail { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? CandidatePhone { get; set; }

    [MaxLength(200)]
    public string JobTitle { get; set; } = string.Empty;

    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    public DateTime AppliedDate { get; set; } = DateTime.Now;

    [MaxLength(500)]
    public string? CoverLetter { get; set; }

    public bool IsViewed { get; set; } = false;

    [MaxLength(50)]
    public string Status { get; set; } = "Pending";
}