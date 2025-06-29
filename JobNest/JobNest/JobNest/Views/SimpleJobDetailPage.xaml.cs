using JobNest.Models;
using JobNest.Services;
using JobNest.Helpers;

namespace JobNest.Views;

public partial class SimpleJobDetailPage : ContentPage
{
    private readonly JobPostViewModel _jobPost;
    private readonly DatabaseService _databaseService;

    public SimpleJobDetailPage(JobPostViewModel jobPost)
    {
        InitializeComponent();
        _jobPost = jobPost;
        _databaseService = ServiceHelper.GetService<DatabaseService>();

        LoadJobDetails();
    }

    private void LoadJobDetails()
    {
        CompanyLabel.Text = _jobPost.CompanyName;
        TitleLabel.Text = _jobPost.Title;
        SalaryLabel.Text = _jobPost.Salary;
        LocationLabel.Text = _jobPost.Location;
        DescriptionLabel.Text = _jobPost.Description;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnApplyClicked(object sender, EventArgs e)
    {
        var currentUserService = ServiceHelper.GetService<CurrentUserService>();
        var currentUser = currentUserService.CurrentUser;

        if (currentUser == null)
        {
            await DisplayAlert("Greška", "Morate biti ulogovani da biste aplicirali za posao.", "OK");
            return;
        }

        if (currentUser.Role != UserRole.Candidate)
        {
            await DisplayAlert("Informacija", "Samo kandidati mogu aplicirati za poslove.", "OK");
            return;
        }

        try
        {
            // Provjeri da li se već prijavio
            var hasApplied = await _databaseService.HasUserAppliedForJobAsync(currentUser.Id, _jobPost.Id);
            if (hasApplied)
            {
                await DisplayAlert("Informacija", "Već ste se prijavili za ovaj posao.", "OK");
                return;
            }

            bool confirm = await DisplayAlert("Potvrda",
                $"Da li ste sigurni da se želite prijaviti za poziciju '{_jobPost.Title}' u kompaniji {_jobPost.CompanyName}?",
                "Da", "Ne");

            if (confirm)
            {
                // Kreiraj job aplikaciju
                var application = new JobApplication
                {
                    JobPostId = _jobPost.Id,
                    CandidateId = currentUser.Id,
                    CandidateName = currentUser.Name,
                    CandidateEmail = currentUser.Email,
                    CandidatePhone = currentUser.Phone,
                    JobTitle = _jobPost.Title,
                    CompanyName = _jobPost.CompanyName,
                    AppliedDate = DateTime.Now,
                    Status = "Pending"
                };

                await _databaseService.CreateJobApplicationAsync(application);
                await DisplayAlert("Uspjeh", "Vaša aplikacija je uspješno poslata! Kompanija će vas kontaktirati.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", $"Problem sa slanjem aplikacije: {ex.Message}", "OK");
        }
    }

    private async Task<User?> GetCurrentUser()
    {
        try
        {
            var testUser = await _databaseService.GetUserByEmailAsync("marko@example.com");
            return testUser;
        }
        catch
        {
            return null;
        }
    }
}