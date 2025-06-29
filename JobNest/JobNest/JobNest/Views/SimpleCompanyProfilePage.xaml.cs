using JobNest.Models;
using JobNest.Services;
using JobNest.Helpers;
using System.Linq;

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
    }

    private async Task LoadCompanyProfile()
    {
        try
        {
            var currentUser = _currentUserService.CurrentUser;
            if (currentUser != null)
            {
                var companyProfile = await _databaseService.GetCompanyProfileByUserIdAsync(currentUser.Id);

                // Company Name - obavezno iz profila ili user imena
                CompanyNameLabel.Text = companyProfile?.CompanyName ?? currentUser.Name ?? "Naziv kompanije";

                // Location - samo ako je unesena u edit profilu  
                if (!string.IsNullOrWhiteSpace(companyProfile?.Location) && companyProfile.Location != "Sarajevo, BiH")
                {
                    LocationLabel.Text = companyProfile.Location;
                    LocationLabel.IsVisible = true;
                }
                else
                {
                    LocationLabel.IsVisible = false;
                }

                // Phone - samo ako je unesen
                if (!string.IsNullOrWhiteSpace(companyProfile?.ContactPhone ?? currentUser.Phone))
                {
                    PhoneLabel.Text = companyProfile?.ContactPhone ?? currentUser.Phone;
                    PhoneSection.IsVisible = true;
                }
                else
                {
                    PhoneSection.IsVisible = false;
                }

                // Email - samo ako je unesen
                if (!string.IsNullOrWhiteSpace(companyProfile?.ContactEmail ?? currentUser.Email))
                {
                    EmailLabel.Text = companyProfile?.ContactEmail ?? currentUser.Email;
                    EmailSection.IsVisible = true;
                }
                else
                {
                    EmailSection.IsVisible = false;
                }

                // Website - samo ako je unesen
                if (!string.IsNullOrWhiteSpace(companyProfile?.Website))
                {
                    WebsiteLabel.Text = companyProfile.Website;
                    WebsiteSection.IsVisible = true;
                }
                else
                {
                    WebsiteSection.IsVisible = false;
                }

                // Description - samo ako postoji
                if (!string.IsNullOrWhiteSpace(companyProfile?.Description))
                {
                    DescriptionLabel.Text = companyProfile.Description;
                    DescriptionSection.IsVisible = true;
                }
                else
                {
                    DescriptionSection.IsVisible = false;
                }

                // Logo - prikaži emoji u Label-u
                CompanyLogoLabel.Text = "🏢";

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

            var allApplications = await _databaseService.GetApplicationsByCompanyAsync(companyId);

            System.Diagnostics.Debug.WriteLine($"Kompanija ima {activeJobsCount} aktivnih poslova i {allApplications.Count} aplikacija");
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
        try
        {
            var currentUser = _currentUserService.CurrentUser;
            if (currentUser != null)
            {
                var existingProfile = await _databaseService.GetCompanyProfileByUserIdAsync(currentUser.Id);
                var editPage = new CompanyProfileEditPage(existingProfile);
                await Navigation.PushAsync(editPage);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", $"Problem sa otvaranjem edit stranice: {ex.Message}", "OK");
        }
    }

    private async void OnLogoClicked(object sender, EventArgs e)
    {
        var action = await DisplayActionSheet(
            "Logo kompanije",
            "Otkaži",
            null,
            "Odaberi sliku iz galerije",
            "Napravi sliku kamerom",
            "Ukloni logo");

        switch (action)
        {
            case "Odaberi sliku iz galerije":
                await PickLogoFromGallery();
                break;
            case "Napravi sliku kamerom":
                await TakeLogoPhoto();
                break;
            case "Ukloni logo":
                await RemoveCompanyLogo();
                break;
        }
    }

    private async Task PickLogoFromGallery()
    {
        try
        {
            var result = await Microsoft.Maui.Media.MediaPicker.PickPhotoAsync();
            if (result != null)
            {
                await SaveCompanyLogo(result);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", $"Problem sa odabirom slike: {ex.Message}", "OK");
        }
    }

    private async Task TakeLogoPhoto()
    {
        try
        {
            var result = await Microsoft.Maui.Media.MediaPicker.CapturePhotoAsync();
            if (result != null)
            {
                await SaveCompanyLogo(result);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", $"Problem sa fotografisanjem: {ex.Message}", "OK");
        }
    }

    private async Task SaveCompanyLogo(FileResult photo)
    {
        try
        {
            var currentUser = _currentUserService.CurrentUser;
            if (currentUser != null)
            {
                // Kreiraj direktorij za logove ako ne postoji
                var logoDirectory = Path.Combine(FileSystem.AppDataDirectory, "company_logos");
                Directory.CreateDirectory(logoDirectory);

                // Generiraj jedinstveno ime fajla
                var fileName = $"logo_{currentUser.Id}_{DateTime.Now:yyyyMMddHHmmss}.jpg";
                var filePath = Path.Combine(logoDirectory, fileName);

                // Kopiraj sliku
                using var stream = await photo.OpenReadAsync();
                using var fileStream = File.Create(filePath);
                await stream.CopyToAsync(fileStream);

                // Updateaj profil u bazi
                var companyProfile = await _databaseService.GetCompanyProfileByUserIdAsync(currentUser.Id);
                if (companyProfile != null)
                {
                    companyProfile.LogoPath = filePath;
                    await _databaseService.UpdateCompanyProfileAsync(companyProfile);
                }

                // Za sada samo pokaži emoji, custom logo implementacija dolazi kasnije
                CompanyLogoLabel.Text = "🏢";

                await DisplayAlert("Uspjeh", "Logo kompanije je uspješno ažuriran!", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", $"Problem sa snimanjem loga: {ex.Message}", "OK");
        }
    }

    private async Task RemoveCompanyLogo()
    {
        try
        {
            var currentUser = _currentUserService.CurrentUser;
            if (currentUser != null)
            {
                var companyProfile = await _databaseService.GetCompanyProfileByUserIdAsync(currentUser.Id);
                if (companyProfile != null)
                {
                    // Obriši stari fajl ako postoji
                    if (!string.IsNullOrEmpty(companyProfile.LogoPath) && File.Exists(companyProfile.LogoPath))
                    {
                        File.Delete(companyProfile.LogoPath);
                    }

                    companyProfile.LogoPath = null;
                    await _databaseService.UpdateCompanyProfileAsync(companyProfile);
                }

                // Vrati na default emoji
                CompanyLogoLabel.Text = "🏢";

                await DisplayAlert("Uspjeh", "Logo kompanije je uklonjen!", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", $"Problem sa uklanjanjem loga: {ex.Message}", "OK");
        }
    }

    private async Task ConfirmLogout()
    {
        var confirm = await DisplayAlert("Potvrda", "Da li se želite odjaviti?", "Da", "Ne");
        if (confirm)
        {
            _currentUserService.Logout();
            Application.Current.MainPage = new NavigationPage(new LoginPage(Models.UserRole.Company));
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Task.Run(LoadCompanyProfile);
    }

    private async void OnMenuClicked(object sender, EventArgs e)
    {
        var action = await DisplayActionSheet(
            "Opcije",
            "Otkaži",
            null,
            "Uredi profil",
            "Odjavi se");

        switch (action)
        {
            case "Uredi profil":
                OnEditProfileClicked(sender, e);
                break;
            case "Odjavi se":
                await ConfirmLogout();
                break;
        }
    }

    private async void OnViewJobsClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    // NAVIGATION BAR METHODS
    private async void OnHomeClicked(object sender, EventArgs e)
    {
        // Idi na CompanyDashboard
        await Navigation.PopToRootAsync();
    }



    private async void OnCandidatesClicked(object sender, EventArgs e)
    {
        // Prikaži kandidate koji su aplicirali
        try
        {
            var currentUser = _currentUserService.CurrentUser;
            if (currentUser != null)
            {
                var applications = await _databaseService.GetApplicationsByCompanyAsync(currentUser.Id);

                if (applications != null && applications.Any())
                {
                    var candidateNames = string.Join("\n", applications.Select(a => $"• {a.CandidateName}"));
                    await DisplayAlert("Kandidati", $"Aplikacije primljene:\n\n{candidateNames}", "OK");
                }
                else
                {
                    await DisplayAlert("Kandidati", "Nema još uvijek primljenih aplikacija.", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", $"Problem sa učitavanjem kandidata: {ex.Message}", "OK");
        }
    }

    private void OnCompanyClicked(object sender, EventArgs e)
    {
        // Već smo na Company profil stranici, ništa ne radimo
    }
}