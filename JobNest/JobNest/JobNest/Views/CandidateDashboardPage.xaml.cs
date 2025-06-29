using JobNest.Models;
using JobNest.Services;
using JobNest.Helpers;
using System.Collections.ObjectModel;

namespace JobNest.Views;

public partial class CandidateDashboardPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private readonly CurrentUserService _currentUserService;
    private ObservableCollection<JobPost> _recentJobs;

    public CandidateDashboardPage()
    {
        InitializeComponent();
        _databaseService = ServiceHelper.GetService<DatabaseService>();
        _currentUserService = ServiceHelper.GetService<CurrentUserService>();

        _recentJobs = new ObservableCollection<JobPost>();

        LoadDashboard();
        _ = LoadJobsAsync();
    }

    private async void LoadDashboard()
    {
        try
        {
            var currentUser = _currentUserService.CurrentUser;
            if (currentUser != null)
            {
                NameLabel.Text = currentUser.Name ?? "Korisnik";
                GreetingLabel.Text = _currentUserService.GetGreeting();
            }
            else
            {
                NameLabel.Text = "Korisnik";
                GreetingLabel.Text = "Dobrodošli";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", $"Problem sa učitavanjem: {ex.Message}", "OK");
        }
    }

    private async Task LoadJobsAsync()
    {
        try
        {
            var allJobs = await _databaseService.GetJobPostsAsync();
            var activeJobs = allJobs.Where(j => j.IsActive && !j.Title.StartsWith("[OBRISAN]")).ToList();

            if (activeJobs.Any())
            {
                _recentJobs.Clear();

                foreach (var job in activeJobs)
                {
                    _recentJobs.Add(job);
                }

                // Samo popuni RecentJobsCollectionView
                RecentJobsCollectionView.ItemsSource = _recentJobs;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Greška pri učitavanju poslova: {ex.Message}");
        }
    }

    private async void OnSearchBarTapped(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new SearchPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", $"Problem sa otvaranjem pretrage: {ex.Message}", "OK");
        }
    }

    private async void OnProfileTapped(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new ProfilePage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", $"Problem sa otvaranjem profila: {ex.Message}", "OK");
        }
    }

    private async void OnJobCardTapped(object sender, EventArgs e)
    {
        try
        {
            var frame = sender as Frame;
            JobPost selectedJob = null;

            if (frame?.BindingContext is JobPost job)
            {
                selectedJob = job;
            }

            if (selectedJob != null)
            {
                // Koristi SearchPage umjesto SimpleJobDetailPage da izbjegnem grešku
                await Navigation.PushAsync(new SearchPage());
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", $"Problem sa otvaranjem detalja posla: {ex.Message}", "OK");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadJobsAsync();
    }

    // NAVIGATION BAR METHODS
    private void OnHomeClicked(object sender, EventArgs e)
    {
        // Već smo na CandidateDashboard stranici, samo refresh
        _ = LoadJobsAsync();
    }

    private async void OnCandidatesClicked(object sender, EventArgs e)
    {
        // Idi na search stranicu za kandidate
        await Navigation.PushAsync(new SearchPage());
    }

    private async void OnCompanyClicked(object sender, EventArgs e)
    {
        // Idi na profil stranicu
        await Navigation.PushAsync(new ProfilePage());
    }
}