using JobNest.Services;
using JobNest.Helpers;

namespace JobNest.Views;

public partial class SimpleCompanyProfilePage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private readonly CurrentUserService _currentUserService;

    public SimpleCompanyProfilePage()
    {
        InitializeComponent();
        _databaseService = ServiceHelper.GetService<DatabaseService>();
        _currentUserService = ServiceHelper.GetService<CurrentUserService>();
        _ = LoadCompanyProfileAsync();
    }

    private async Task LoadCompanyProfileAsync()
    {
        try
        {
            var currentUser = _currentUserService.CurrentUser;
            if (currentUser != null)
            {
                CompanyNameLabel.Text = currentUser.Name ?? "Naziv kompanije";
                EmailLabel.Text = currentUser.Email ?? "info@kompanija.ba";

                await LoadCompanyStatistics(currentUser.Id);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", $"Problem sa učitavanjem profila: {ex.Message}", "OK");
        }
    }

    private async Task LoadCompanyStatistics(int companyId)
    {
        try
        {
            var activeJobs = await _databaseService.GetJobPostsByCompanyAsync(companyId);
            var activeJobsCount = activeJobs.Where(j => j.IsActive && !j.Title.StartsWith("[OBRISAN]")).Count();
            ActiveJobsCountLabel.Text = activeJobsCount.ToString();

            var allApplications = await _databaseService.GetApplicationsByCompanyAsync(companyId);
            TotalApplicationsLabel.Text = allApplications.Count.ToString();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Greška pri učitavanju statistika: {ex.Message}");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnEditProfileClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Uskoro", "Uređivanje profila kompanije će biti dostupno uskoro.", "OK");
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        var action = await DisplayActionSheet(
            "Postavke kompanije",
            "Otkaži",
            null,
            "Promijeni password",
            "Notifikacije",
            "Privatnost",
            "Odjavi se");

        switch (action)
        {
            case "Promijeni password":
                await DisplayAlert("Uskoro", "Promjena password-a će biti dostupna uskoro.", "OK");
                break;
            case "Notifikacije":
                await DisplayAlert("Uskoro", "Postavke notifikacija će biti dostupne uskoro.", "OK");
                break;
            case "Privatnost":
                await DisplayAlert("Uskoro", "Postavke privatnosti će biti dostupne uskoro.", "OK");
                break;
            case "Odjavi se":
                await ConfirmLogout();
                break;
        }
    }

    private async Task ConfirmLogout()
    {
        var confirm = await DisplayAlert("Potvrda", "Da li se želite odjaviti?", "Da", "Ne");
        if (confirm)
        {
            _currentUserService.Logout();
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadCompanyProfileAsync();
    }
}