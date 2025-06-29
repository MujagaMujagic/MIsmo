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

    public async Task InitializeDatabaseAsync()
    {
        // Obriši postojeću bazu i kreiraj novu sa svim kolonama
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        // Dodavanje test podataka
        await SeedTestDataAsync();
    }

    public async Task<User> CreateUserAsync(string email, string password, UserRole role, string firstName = "", string lastName = "", string companyName = "", string phoneNumber = "")
    {
        // Provjeri da li email već postoji
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Korisnik sa ovim email-om već postoji");
        }

        // Jednostavan User bez dodatnih profila
        var user = new User
        {
            Name = !string.IsNullOrEmpty(companyName) ? companyName : $"{firstName} {lastName}".Trim(),
            Email = email,
            Phone = phoneNumber,
            PasswordHash = HashPassword(password),
            Role = role,
            IsActive = true,
            DateCreated = DateTime.UtcNow
        };

        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Greška pri kreiranju korisnika: {ex.Message}", ex);
        }
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
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

    // Glavna metoda za kreiranje job post-a
    public async Task<JobPost> CreateJobPostAsync(JobPost jobPost)
    {
        _context.JobPosts.Add(jobPost);
        await _context.SaveChangesAsync();
        return jobPost;
    }

    // Legacy metoda za kompatibilnost
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

    private async Task SeedTestDataAsync()
    {
        // Provjeri da li već postoje podaci
        if (await _context.Users.AnyAsync())
            return;

        // Test korisnici - jednostavno bez profila
        var testUsers = new List<User>
        {
            new User
            {
                Name = "Tech Solutions BiH",
                Email = "test@kompanija.ba",
                PasswordHash = HashPassword("Test123!"),
                Role = UserRole.Company,
                IsActive = true,
                DateCreated = DateTime.UtcNow
            },
            new User
            {
                Name = "Digital Agency Five",
                Email = "five@zagreb.hr",
                PasswordHash = HashPassword("Test123!"),
                Role = UserRole.Company,
                IsActive = true,
                DateCreated = DateTime.UtcNow
            },
            new User
            {
                Name = "Crvtko Solutions",
                Email = "crvtko@tuzla.ba",
                PasswordHash = HashPassword("Test123!"),
                Role = UserRole.Company,
                IsActive = true,
                DateCreated = DateTime.UtcNow
            },
            new User
            {
                Name = "Marko Petrović",
                Email = "marko@example.com",
                PasswordHash = HashPassword("Test123!"),
                Role = UserRole.Candidate,
                IsActive = true,
                DateCreated = DateTime.UtcNow
            }
        };

        _context.Users.AddRange(testUsers);
        await _context.SaveChangesAsync();

        // Test poslovi sa novim svojstvima
        var testJobs = new List<JobPost>
        {
            new JobPost
            {
                CompanyId = 1, // Tech Solutions BiH
                Title = "Software Engineer",
                Description = "Tražimo iskusnog software engineera za rad na modernim web aplikacijama. Potrebno je znanje React, Node.js i PostgreSQL baza podataka.",
                Location = "Sarajevo, BiH",
                Salary = "1500 KM - 2500 KM",
                Category = "IT i tehnologija",
                JobType = "Puno radno vrijeme",
                SalaryMin = 1500,
                SalaryMax = 2500,
                Requirements = "3+ godina iskustva, React, Node.js, PostgreSQL",
                Benefits = "Zdravstveno osiguranje, fleksibilno radno vrijeme",
                Company = "Tech Solutions BiH",
                PostedDate = DateTime.UtcNow,
                IsActive = true
            },
            new JobPost
            {
                CompanyId = 1,
                Title = "Junior Software Engineer",
                Description = "Odličan posao za početnike u IT industriji. Mentorstvo od seniora, rad na zanimljivim projektima.",
                Location = "Sarajevo, BiH",
                Salary = "1400 KM - 2500 KM",
                Category = "IT i tehnologija",
                JobType = "Puno radno vrijeme",
                SalaryMin = 1400,
                SalaryMax = 2500,
                Requirements = "Osnovno znanje programiranja, želja za učenjem",
                Benefits = "Mentorstvo, obuke, karrijerni razvoj",
                Company = "Tech Solutions BiH",
                PostedDate = DateTime.UtcNow.AddDays(-1),
                IsActive = true
            },
            new JobPost
            {
                CompanyId = 2, // Digital Agency Five
                Title = "Web & Mobile Development",
                Description = "Five Zagreb je digitalna agencija koja se bavi razvojem softvera, dizajnom i strateških digitalnih rješenja.",
                Location = "Zagreb, Hrvatska",
                Salary = "3000 KM - 5400 KM",
                Category = "IT i tehnologija",
                JobType = "Puno radno vrijeme",
                SalaryMin = 3000,
                SalaryMax = 5400,
                Requirements = "5+ godina iskustva, React Native, Flutter",
                Benefits = "Remote rad, bonusi, moderna oprema",
                Company = "Digital Agency Five",
                PostedDate = DateTime.UtcNow.AddDays(-2),
                IsActive = true
            }
        };

        _context.JobPosts.AddRange(testJobs);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateJobPostAsync(JobPost jobPost)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== AŽURIRANJE POSLA ===");
            System.Diagnostics.Debug.WriteLine($"ID: {jobPost.Id}");
            System.Diagnostics.Debug.WriteLine($"Title: '{jobPost.Title}'");
            System.Diagnostics.Debug.WriteLine($"SalaryMin: {jobPost.SalaryMin}");
            System.Diagnostics.Debug.WriteLine($"SalaryMax: {jobPost.SalaryMax}");
            System.Diagnostics.Debug.WriteLine($"IsActive: {jobPost.IsActive}");

            // Provjeri da li posao postoji
            var existingJob = await _context.JobPosts.FindAsync(jobPost.Id);
            if (existingJob == null)
            {
                System.Diagnostics.Debug.WriteLine($"GREŠKA: Posao sa ID {jobPost.Id} NIJE PRONAĐEN!");
                throw new Exception($"Posao sa ID {jobPost.Id} ne postoji u bazi");
            }

            System.Diagnostics.Debug.WriteLine($"=== POSTOJEĆI PODACI ===");
            System.Diagnostics.Debug.WriteLine($"Stari Title: '{existingJob.Title}'");
            System.Diagnostics.Debug.WriteLine($"Stara SalaryMin: {existingJob.SalaryMin}");
            System.Diagnostics.Debug.WriteLine($"Stara SalaryMax: {existingJob.SalaryMax}");

            // Explicit ažuriranje svih polja
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

            System.Diagnostics.Debug.WriteLine($"=== NOVI PODACI ===");
            System.Diagnostics.Debug.WriteLine($"Novi Title: '{existingJob.Title}'");
            System.Diagnostics.Debug.WriteLine($"Nova SalaryMin: {existingJob.SalaryMin}");
            System.Diagnostics.Debug.WriteLine($"Nova SalaryMax: {existingJob.SalaryMax}");

            // Force EntityFramework da detektuje promjene
            _context.Entry(existingJob).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

            var changes = await _context.SaveChangesAsync();
            System.Diagnostics.Debug.WriteLine($"SaveChanges vratilo: {changes} promjena");

            if (changes > 0)
            {
                System.Diagnostics.Debug.WriteLine("✅ POSAO USPJEŠNO AŽURIRAN!");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠️ NEMA PROMJENA - možda su podaci isti");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ GREŠKA u UpdateJobPostAsync: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task DeleteJobPostAsync(int jobId)
    {
        try
        {
            var jobPost = await _context.JobPosts.FindAsync(jobId);
            if (jobPost != null)
            {
                // Prvo obriši sve aplikacije za ovaj posao
                var applications = _context.JobApplications.Where(a => a.JobPostId == jobId);
                _context.JobApplications.RemoveRange(applications);

                // Zatim obriši posao
                _context.JobPosts.Remove(jobPost);
                await _context.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine($"Posao ID {jobId} je uspješno obrisan sa svim aplikacijama");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Greška pri brisanju posla: {ex.Message}");
            throw;
        }
    }

    // Job Application metode
    public async Task CreateJobApplicationAsync(JobApplication application)
    {
        _context.JobApplications.Add(application);
        await _context.SaveChangesAsync();

        // Debug: Provjeri da li je aplikacija stvarno spremljena
        var count = await _context.JobApplications.CountAsync(a => a.JobPostId == application.JobPostId);
        System.Diagnostics.Debug.WriteLine($"Job {application.JobPostId} sada ima {count} aplikacija");
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
        return await GetApplicationsByJobIdAsync(jobId); // Alias za gornju funkciju
    }

    public async Task<JobPost?> GetJobPostByIdAsync(int jobId)
    {
        return await _context.JobPosts.FindAsync(jobId);
    }

    // Company Profile metode
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
        // Dobij sve job postove kompanije
        var companyJobIds = await _context.JobPosts
            .Where(jp => jp.CompanyId == companyId)
            .Select(jp => jp.Id)
            .ToListAsync();

        // Dobij sve aplikacije za te job postove
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