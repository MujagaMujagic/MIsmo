namespace JobNest.Models
{
    public class JobRecommendation
    {
        public int Id { get; set; }

        public int CandidateId { get; set; }

        public int JobPostId { get; set; }

        public string? Reason { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public CandidateProfile Candidate { get; set; } = null!;
        public JobPost JobPost { get; set; } = null!;
    }
}