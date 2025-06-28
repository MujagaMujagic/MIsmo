using JobNest.Models;
using JobNest.Services;
using JobNest.Helpers;
using System.Collections.ObjectModel;

namespace JobNest.Views;

public partial class JobApplicationsPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private readonly JobPost _jobPost;
    private ObservableCollection<JobApplication> _applications;

    public JobApplicationsPage(JobPost jobPost)
    {
        InitializeComponent();
        _databaseService = ServiceHelper.GetService<DatabaseService>();
        _jobPost = jobPost;
        _applications = new ObservableCollection<JobApplication>();

        InitializePage();
        LoadApplications();
    }

    private void InitializePage()
    {
        JobTitleLabel.Text = _jobPost.Title;
        ApplicationCountLabel.Text = $"{_jobPost.ApplicationCount} aplikacija";
    }

    private async void LoadApplications()
    {
        try
        {
            var applications = await _databaseService.GetApplicationsByJobIdAsync(_jobPost.Id);

            _applications.Clear();
            foreach (var app in applications)
            {
                _applications.Add(app);
            }

            ApplicationsCollectionView.ItemsSource = _applications;

            // Prikaži empty state ako nema aplikacija
            EmptyStateLayout.IsVisible = applications.Count == 0;
            ApplicationsCollectionView.IsVisible = applications.Count > 0;

            // Ažuriraj broj aplikacija
            ApplicationCountLabel.Text = $"{applications.Count} aplikacija";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", $"Problem sa učitavanjem aplikacija: {ex.Message}", "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}