using JobNest.Models;
using JobNest.Services;
using JobNest.Helpers;

namespace JobNest.Views;

public partial class CandidateDashboardPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private readonly CurrentUserService _currentUserService;

    public CandidateDashboardPage()
    {
        InitializeComponent();
        _databaseService = ServiceHelper.GetService<DatabaseService>();
        _currentUserService = ServiceHelper.GetService<CurrentUserService>();
        LoadDashboard();
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

    private async void OnSearchBarTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SearchPage());
    }

    private async void OnProfileTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ProfilePage());
    }
}