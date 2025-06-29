using Microsoft.EntityFrameworkCore;
using JobNest.Data;
using JobNest.Models;
using System.Security.Cryptography;
using System.Text;

namespace JobNest.Services;

public class DatabaseService
{
    private readonly AppDbContext _context;

    public DatabaseService(AppDbContext context)
    {
        _context = context;
    }

    // OVA METODA VIŠE NE BRIŠE BAZU
    public async Task EnsureDatabaseCreatedAsync()
    {
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task<User> CreateUserAsync(string email, string password, UserRole role, string firstName = "", string lastName = "", string companyName = "")
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (existingUser != null)
            throw new InvalidOperationException("Korisnik sa ovim email-om već postoji");

        var user = new User
        {
            Name = !string.IsNullOrEmpty(companyName) ? companyName : $"{firstName} {lastName}".Trim(),
            Email = email,
            PasswordHash = HashPassword(password),
            Role = role,
            IsActive = true,
            DateCreated = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> ValidateUserCredentialsAsync(string email, string password)
    {
        var user = await GetUserByEmailAsync(email);
        return user != null && VerifyPassword(password, user.PasswordHash);
    }

    public async Task<List<JobPost>> GetJobPostsAsync()
    {
        return await _context.JobPosts
            .Where(j => j.IsActive)
            .OrderByDescending(j => j.PostedDate)
            .ToListAsync();
    }

    public async Task<JobPost> CreateJobPostAsync(JobPost jobPost)
    {
        _context.JobPosts.Add(jobPost);
        await _context.SaveChangesAsync();
        return jobPost;
    }

    public async Task<JobPost> CreateJobPostAsync(int companyId, string title, string description, string location, string salary)
    {
        var jobPost = new JobPost
        {
            CompanyId = companyId,
            Title = title,
            Description = description,
            Location = location,
            Salary = salary,
            PostedDate = DateTime.UtcNow,
            IsActive = true
        };

        return await CreateJobPostAsync(jobPost);
    }

    public async Task<List<JobPost>> GetJobPostsByCompanyAsync(int companyId)
    {
        return await _context.JobPosts
            .Where(j => j.CompanyId == companyId)
            .OrderByDescending(j => j.PostedDate)
            .ToListAsync();
    }

    public async Task UpdateJobPostAsync(JobPost jobPost)
    {
        var existingJob = await _context.JobPosts.FindAsync(jobPost.Id);
        if (existingJob == null)
            throw new Exception($"Posao sa ID {jobPost.Id} ne postoji u bazi");

        existingJob.Title = jobPost.Title ?? existingJob.Title;
        existingJob.Description = jobPost.Description ?? existingJob.Description;
        existingJob.Category = jobPost.Category ?? existingJob.Category;
        existingJob.Location = jobPost.Location ?? existingJob.Location;
        existingJob.JobType = jobPost.JobType ?? existingJob.JobType;
        existingJob.SalaryMin = jobPost.SalaryMin;
        existingJob.SalaryMax = jobPost.SalaryMax;
        existingJob.Requirements = jobPost.Requirements ?? existingJob.Requirements;
        existingJob.Benefits = jobPost.Benefits ?? existingJob.Benefits;
        existingJob.IsActive = jobPost.IsActive;

        _context.Entry(existingJob).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteJobPostAsync(int jobId)
    {
        var jobPost = await _context.JobPosts.FindAsync(jobId);
        if (jobPost != null)
        {
            var applications = _context.JobApplications.Where(a => a.JobPostId == jobId);
            _context.JobApplications.RemoveRange(applications);

            _context.JobPosts.Remove(jobPost);
            await _context.SaveChangesAsync();
        }
    }

    public async Task CreateJobApplicationAsync(JobApplication application)
    {
        _context.JobApplications.Add(application);
        await _context.SaveChangesAsync();
    }

    public async Task<List<JobApplication>> GetApplicationsByJobIdAsync(int jobId)
    {
        return await _context.JobApplications
            .Where(a => a.JobPostId == jobId)
            .OrderByDescending(a => a.AppliedDate)
            .ToListAsync();
    }

    public async Task<List<JobApplication>> GetApplicationsForJobAsync(int jobId)
    {
        return await GetApplicationsByJobIdAsync(jobId);
    }

    public async Task<JobPost?> GetJobPostByIdAsync(int jobId)
    {
        return await _context.JobPosts.FindAsync(jobId);
    }

    public async Task<CompanyProfile?> GetCompanyProfileByUserIdAsync(int userId)
    {
        return await _context.CompanyProfiles
            .FirstOrDefaultAsync(cp => cp.UserId == userId);
    }

    public async Task CreateCompanyProfileAsync(CompanyProfile profile)
    {
        _context.CompanyProfiles.Add(profile);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCompanyProfileAsync(CompanyProfile profile)
    {
        _context.CompanyProfiles.Update(profile);
        await _context.SaveChangesAsync();
    }

    public async Task<List<JobApplication>> GetApplicationsByCompanyAsync(int companyId)
    {
        var companyJobIds = await _context.JobPosts
            .Where(jp => jp.CompanyId == companyId)
            .Select(jp => jp.Id)
            .ToListAsync();

        return await _context.JobApplications
            .Where(a => companyJobIds.Contains(a.JobPostId))
            .OrderByDescending(a => a.AppliedDate)
            .ToListAsync();
    }

    public async Task<bool> HasUserAppliedForJobAsync(int candidateId, int jobId)
    {
        return await _context.JobApplications
            .AnyAsync(a => a.CandidateId == candidateId && a.JobPostId == jobId);
    }

    public async Task<int> GetApplicationCountForJobAsync(int jobId)
    {
        return await _context.JobApplications
            .CountAsync(a => a.JobPostId == jobId);
    }

    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "JobNestSalt"));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    private bool VerifyPassword(string password, string hash)
    {
        var hashedPassword = HashPassword(password);
        return hashedPassword == hash;
    }
}
