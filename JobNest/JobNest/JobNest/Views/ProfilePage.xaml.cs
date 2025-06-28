using JobNest.Models;
using JobNest.Services;
using JobNest.Helpers;

namespace JobNest.Views;

public partial class ProfilePage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private readonly CurrentUserService _currentUserService;

    public ProfilePage()
    {
        InitializeComponent();
        _databaseService = ServiceHelper.GetService<DatabaseService>();
        _currentUserService = ServiceHelper.GetService<CurrentUserService>();
        LoadProfile();
    }

    private async void LoadProfile()
    {
        try
        {
            var currentUser = _currentUserService.CurrentUser;
            if (currentUser != null)
            {
                UserNameLabel.Text = currentUser.Name ?? "Korisnik";
                EmailLabel.Text = currentUser.Email ?? "Email nije naveden";
                PhoneLabel.Text = "+387 61 123 456";
                CvStatusLabel.Text = "CV nije učitan";
            }
            else
            {
                UserNameLabel.Text = "Korisnik";
                EmailLabel.Text = "Email nije naveden";
                PhoneLabel.Text = "+387 61 123 456";
                CvStatusLabel.Text = "CV nije učitan";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", $"Greška: {ex.Message}", "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnCvMenuClicked(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("CV", "Želite li uploadovati CV?", "Da", "Ne");
        if (answer)
        {
            var currentUser = _currentUserService.CurrentUser;
            string cvFileName = currentUser != null ? 
                $"CV_{currentUser.Name?.Replace(" ", "_")}.pdf" : 
                "CV_Korisnik.pdf";
            
            CvStatusLabel.Text = cvFileName;
            await DisplayAlert("Uspjeh", "CV je uploadovan.", "OK");
        }
    }

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//CandidateDashboard");
    }

    private async void OnSearchClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SearchPage());
    }

    private async void OnNotificationsClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Obavijesti", "Funkcionalnost će biti dostupna uskoro.", "OK");
    }
}