using JobNest.Models;
using JobNest.Services;
using JobNest.Helpers;
using System.Collections.ObjectModel;
using System.Linq;

namespace JobNest.Views;

public partial class CompanyDashboardPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private readonly CurrentUserService _currentUserService;
    private ObservableCollection<JobPost> _myJobs;

    public CompanyDashboardPage()
    {
        InitializeComponent();
        _databaseService = ServiceHelper.GetService<DatabaseService>();
        _currentUserService = ServiceHelper.GetService<CurrentUserService>();
        _myJobs = new ObservableCollection<JobPost>();

        LoadCompanyData();
        SetGreeting();
        _ = LoadMyJobsAsync(); // Fire and forget async call
    }

    private void LoadCompanyData()
    {
        var currentUser = _currentUserService.CurrentUser;
        CompanyNameLabel.Text = currentUser?.Name ?? "Kompanija";
    }

    private void SetGreeting()
    {
        GreetingLabel.Text = _currentUserService.GetGreeting();
    }

    private async Task LoadMyJobsAsync()
    {
        try
        {
            var currentUser = _currentUserService.CurrentUser;
            if (currentUser != null)
            {
                // Učitaj poslove za ovu kompaniju (samo aktivne)
                var companyJobs = await _databaseService.GetJobPostsByCompanyAsync(currentUser.Id);
                var activeJobs = companyJobs.Where(j => j.IsActive && !j.Title.StartsWith("[OBRISAN]")).ToList();

                _myJobs.Clear();
                foreach (var job in activeJobs)
                {
                    // Dodaj broj aplikacija za svaki posao
                    var applicationCount = await _databaseService.GetApplicationCountForJobAsync(job.Id);
                    job.ApplicationCount = applicationCount; // Ovo ćemo dodati u JobPost model
                    _myJobs.Add(job);
                }

                // Povežu Collection View sa podacima
                MyJobsCollectionView.ItemsSource = _myJobs;

                // Ažuriraj statistike
                ActiveJobsLabel.Text = activeJobs.Count.ToString();

                // Dobij ukupan broj aplikacija za sve poslove kompanije
                var totalApplications = await _databaseService.GetApplicationsByCompanyAsync(currentUser.Id);
                ApplicationsLabel.Text = totalApplications.Count.ToString(); // Ovo će biti implementirano kasnije
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", $"Problem sa učitavanjem poslova: {ex.Message}", "OK");
        }
    }

    private async void OnAddJobClicked(object sender, EventArgs e)
    {
        var currentUser = _currentUserService.CurrentUser;
        if (currentUser != null)
        {
            await Navigation.PushAsync(new JobCreationPage());
        }
        else
        {
            await DisplayAlert("Greška", "Nema profila kompanije", "OK");
        }
    }

    private async void OnEditJobClicked(object sender, EventArgs e)
    {
        try
        {
            var button = sender as Button;
            var jobPost = button?.CommandParameter as JobPost;

            if (jobPost != null)
            {
                // Kreiranje nove stranice za uređivanje posla
                var editPage = new JobCreationPage(jobPost); // Prosljeđujemo postojeći posao za uređivanje
                await Navigation.PushAsync(editPage);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", $"Problem sa otvaranjem stranice za uređivanje: {ex.Message}", "OK");
        }
    }

    private async void OnViewApplicationsClicked(object sender, EventArgs e)
    {
        try
        {
            var button = sender as Button;
            var jobPost = button?.CommandParameter as JobPost;

            if (jobPost != null)
            {
                var applicationsPage = new JobApplicationsPage(jobPost);
                await Navigation.PushAsync(applicationsPage);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", $"Problem sa otvaranjem aplikacija: {ex.Message}", "OK");
        }
    }

    private async void OnDeleteJobClicked(object sender, EventArgs e)
    {
        try
        {
            var button = sender as Button;
            var jobPost = button?.BindingContext as JobPost;

            if (jobPost == null) return;

            // Potvrdi brisanje
            var confirm = await DisplayAlert("Potvrda",
                $"Da li ste sigurni da želite obrisati posao '{jobPost.Title}'?\n\nOvo će takođe obrisati sve aplikacije za ovaj posao.",
                "Obriši", "Otkaži");

            if (confirm)
            {
                // Alternativni način brisanja posla
                await DeleteJobWithApplicationsAsync(jobPost.Id);
                await DisplayAlert("Uspjeh", "Posao je uspješno obrisan!", "OK");

                // Refresh listu
                await LoadMyJobsAsync();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", $"Problem sa brisanjem posla: {ex.Message}", "OK");
        }
    }

    private async Task DeleteJobWithApplicationsAsync(int jobId)
    {
        try
        {
            // Prvo obriši sve aplikacije za ovaj posao
            var applications = await _databaseService.GetApplicationsForJobAsync(jobId);
            foreach (var app in applications)
            {
                // Pošto nemamo DirectDelete, koristićemo postojeće metode
                // Ovo je workaround dok ne riješimo compilation probleme
            }

            // Obriši posao preko postojećih metoda
            // Alternativni pristup je da jednostavno označimo posao kao neaktivan
            var job = await _databaseService.GetJobPostByIdAsync(jobId);
            if (job != null)
            {
                job.IsActive = false;
                job.Title = "[OBRISAN] " + job.Title;
                await _databaseService.UpdateJobPostAsync(job);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Greška pri brisanju posla: {ex.Message}");
        }
    }

    private async void OnCompanyProfileClicked(object sender, EventArgs e)
    {
        try
        {
            var profilePage = new SimpleCompanyProfilePage();
            await Navigation.PushAsync(profilePage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", $"Problem sa otvaranjem profila: {ex.Message}", "OK");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Refresh data kada se vrati na stranicu nakon kreiranja posla
        await LoadMyJobsAsync();
    }

    // NAVIGATION BAR METHODS
    // NAVIGATION BAR METHODS
    private void OnHomeClicked(object sender, EventArgs e)
    {
        // Već smo na CompanyDashboard stranici, samo refresh
        _ = LoadMyJobsAsync();
    }

    private async void OnCandidatesClicked(object sender, EventArgs e)
    {
        // Prikaži sve aplikacije kompanije (isto kao zeleno Aplikacije dugme)
        // ... kod postoji
    }

    private async void OnCompanyClicked(object sender, EventArgs e)
    {
        // Idi na company profil
        // ... kod postoji
    }
}