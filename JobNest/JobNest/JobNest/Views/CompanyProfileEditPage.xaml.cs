using JobNest.Models;
using JobNest.Services;
using JobNest.Helpers;

namespace JobNest.Views;

public partial class CompanyProfileEditPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private readonly CurrentUserService _currentUserService;
    private CompanyProfile? _companyProfile;
    private bool _isNewProfile;

    public CompanyProfileEditPage(CompanyProfile? existingProfile = null)
    {
        InitializeComponent();
        _databaseService = ServiceHelper.GetService<DatabaseService>();
        _currentUserService = ServiceHelper.GetService<CurrentUserService>();

        _companyProfile = existingProfile;
        _isNewProfile = existingProfile == null;

        LoadProfileData();
    }

    private void LoadProfileData()
    {
        var currentUser = _currentUserService.CurrentUser;

        if (_companyProfile != null)
        {
            // Popuni formu sa postojećim podacima
            CompanyNameEntry.Text = _companyProfile.CompanyName;
            IndustryPicker.SelectedItem = _companyProfile.Industry;
            LocationEntry.Text = _companyProfile.Location;
            EmployeeCountPicker.SelectedItem = _companyProfile.EmployeeCount;
            ContactEmailEntry.Text = _companyProfile.ContactEmail;
            ContactPhoneEntry.Text = _companyProfile.ContactPhone;
            WebsiteEntry.Text = _companyProfile.Website;
            DescriptionEditor.Text = _companyProfile.Description;
        }
        else if (currentUser != null)
        {
            // Za novi profil, popuni osnovne podatke iz User
            CompanyNameEntry.Text = currentUser.Name;
            ContactEmailEntry.Text = currentUser.Email;
            ContactPhoneEntry.Text = currentUser.Phone;
            IndustryPicker.SelectedItem = "IT & Software";
            LocationEntry.Text = "Sarajevo, BiH";
            EmployeeCountPicker.SelectedItem = "1-10";
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (!await ValidateFormAsync())
            return;

        try
        {
            var currentUser = _currentUserService.CurrentUser;
            if (currentUser == null)
            {
                await DisplayAlert("Greška", "Morate biti ulogovani.", "OK");
                return;
            }

            if (_isNewProfile)
            {
                // Kreiraj novi profil
                var newProfile = new CompanyProfile
                {
                    UserId = currentUser.Id,
                    CompanyName = CompanyNameEntry.Text?.Trim(),
                    Industry = IndustryPicker.SelectedItem?.ToString(),
                    Location = LocationEntry.Text?.Trim(),
                    EmployeeCount = EmployeeCountPicker.SelectedItem?.ToString(),
                    ContactEmail = ContactEmailEntry.Text?.Trim(),
                    ContactPhone = ContactPhoneEntry.Text?.Trim(),
                    Website = WebsiteEntry.Text?.Trim(),
                    Description = DescriptionEditor.Text?.Trim(),
                    DateCreated = DateTime.UtcNow
                };

                await _databaseService.CreateCompanyProfileAsync(newProfile);
                await DisplayAlert("Uspjeh", "Profil kompanije je kreiran!", "OK");
            }
            else
            {
                // Ažuriraj postojeći profil
                _companyProfile.CompanyName = CompanyNameEntry.Text?.Trim();
                _companyProfile.Industry = IndustryPicker.SelectedItem?.ToString();
                _companyProfile.Location = LocationEntry.Text?.Trim();
                _companyProfile.EmployeeCount = EmployeeCountPicker.SelectedItem?.ToString();
                _companyProfile.ContactEmail = ContactEmailEntry.Text?.Trim();
                _companyProfile.ContactPhone = ContactPhoneEntry.Text?.Trim();
                _companyProfile.Website = WebsiteEntry.Text?.Trim();
                _companyProfile.Description = DescriptionEditor.Text?.Trim();

                await _databaseService.UpdateCompanyProfileAsync(_companyProfile);
                await DisplayAlert("Uspjeh", "Profil kompanije je ažuriran!", "OK");
            }

            // Vrati se na profil stranicu
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", $"Problem sa čuvanjem profila: {ex.Message}", "OK");
        }
    }

    private async Task<bool> ValidateFormAsync()
    {
        if (string.IsNullOrWhiteSpace(CompanyNameEntry.Text))
        {
            await DisplayAlert("Greška", "Naziv kompanije je obavezan.", "OK");
            return false;
        }

        if (IndustryPicker.SelectedItem == null)
        {
            await DisplayAlert("Greška", "Molimo odaberite industriju.", "OK");
            return false;
        }

        if (string.IsNullOrWhiteSpace(LocationEntry.Text))
        {
            await DisplayAlert("Greška", "Lokacija je obavezna.", "OK");
            return false;
        }

        // Validacija email format-a
        if (!string.IsNullOrWhiteSpace(ContactEmailEntry.Text))
        {
            var email = ContactEmailEntry.Text.Trim();
            if (!email.Contains("@") || !email.Contains("."))
            {
                await DisplayAlert("Greška", "Molimo unesite validnu email adresu.", "OK");
                return false;
            }
        }

        // Validacija website format-a
        if (!string.IsNullOrWhiteSpace(WebsiteEntry.Text))
        {
            var website = WebsiteEntry.Text.Trim();
            if (!website.Contains("."))
            {
                await DisplayAlert("Greška", "Molimo unesite validnu web adresu.", "OK");
                return false;
            }
        }

        return true;
    }

    private async void OnLogoTapped(object sender, EventArgs e)
    {
        // Logo upload funkcionalnost - trenutno samo emoji
    }
    // NAVIGATION BAR METHODS
    private async void OnHomeClicked(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }

    private async void OnCandidatesClicked(object sender, EventArgs e)
    {
        try
        {
            var currentUser = _currentUserService.CurrentUser;
            if (currentUser != null)
            {
                var applications = await _databaseService.GetApplicationsByCompanyAsync(currentUser.Id);

                if (applications != null && applications.Any())
                {
                    var candidateNames = string.Join("\n", applications.Select(a => $"• {a.CandidateName}"));
                    await DisplayAlert("Kandidati", $"Aplikacije:\n\n{candidateNames}", "OK");
                }
                else
                {
                    await DisplayAlert("Kandidati", "Nema aplikacija.", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", ex.Message, "OK");
        }
    }

    private async void OnCompanyClicked(object sender, EventArgs e)
    {
        try
        {
            var profilePage = new SimpleCompanyProfilePage();
            await Navigation.PushAsync(profilePage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", ex.Message, "OK");
        }
    }
}