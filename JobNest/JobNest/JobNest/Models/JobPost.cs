using System.ComponentModel.DataAnnotations;

namespace JobNest.Models;

public class JobPost
{
    public int Id { get; set; }

    public int CompanyId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Location { get; set; }

    [MaxLength(100)]
    public string? Salary { get; set; }

    // Nova svojstva za detaljnije job post-ove
    [MaxLength(100)]
    public string? Category { get; set; }

    [MaxLength(50)]
    public string? JobType { get; set; }

    public decimal? SalaryMin { get; set; }

    public decimal? SalaryMax { get; set; }

    [MaxLength(1000)]
    public string? Requirements { get; set; }

    [MaxLength(1000)]
    public string? Benefits { get; set; }

    [MaxLength(200)]
    public string? Company { get; set; }

    // Alias za UI binding kompatibilnost
    public string? CompanyName => Company;

    public DateTime PostedDate { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiryDate { get; set; }

    public bool IsActive { get; set; } = true;

    // Za UI prikaz - neće se čuvati u bazi
    public int ApplicationCount { get; set; } = 0;

    // Navigation properties uklonjene za jednostavnost
}