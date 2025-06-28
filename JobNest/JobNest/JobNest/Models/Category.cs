using System.ComponentModel.DataAnnotations;

namespace JobNest.Models;

public class Category
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<JobPost> JobPosts { get; set; } = new List<JobPost>();
}