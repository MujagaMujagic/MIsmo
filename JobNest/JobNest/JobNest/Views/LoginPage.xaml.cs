using JobNest.Models;
using JobNest.Services;
using JobNest.Helpers;

namespace JobNest.Views;

public partial class LoginPage : ContentPage
{
    private bool _isPasswordVisible = false;
    private UserRole _selectedRole = UserRole.Candidate;
    private readonly DatabaseService _databaseService;
    private readonly CurrentUserService _currentUserService;

    public LoginPage()
    {
        InitializeComponent();
        _databaseService = ServiceHelper.GetService<DatabaseService>();
        _currentUserService = ServiceHelper.GetService<CurrentUserService>();
        SetupUI(UserRole.Candidate);
    }

    public LoginPage(UserRole userRole)
    {
        InitializeComponent();
        _databaseService = ServiceHelper.GetService<DatabaseService>();
        _currentUserService = ServiceHelper.GetService<CurrentUserService>();
        SetupUI(userRole);
    }

    private void SetupUI(UserRole userRole)
    {
        _selectedRole = userRole;
        RoleLabel.Text = _selectedRole == UserRole.Candidate ? "Kandidat" : "Kompanija";
    }

    private void OnShowPasswordClicked(object sender, EventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        PasswordEntry.IsPassword = !_isPasswordVisible;
        ShowPasswordButton.Text = _isPasswordVisible ? "🙈" : "👁";
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(EmailEntry.Text) || string.IsNullOrEmpty(PasswordEntry.Text))
        {
            await DisplayAlert("Greška", "Molimo unesite email i lozinku", "OK");
            return;
        }

        try
        {
            // Provjeri korisničke podatke u bazi
            bool isValid = await _databaseService.ValidateUserCredentialsAsync(EmailEntry.Text, PasswordEntry.Text);

            if (!isValid)
            {
                await DisplayAlert("Greška", "Neispravni podaci za prijavu", "OK");
                return;
            }

            // Dobij korisnika iz baze
            var user = await _databaseService.GetUserByEmailAsync(EmailEntry.Text);

            if (user == null)
            {
                await DisplayAlert("Greška", "Korisnik nije pronađen", "OK");
                return;
            }

            // Postavi trenutnog korisnika
            _currentUserService.CurrentUser = user;

            // Navigiraj na Dashboard na osnovu uloge
            if (user.Role == UserRole.Candidate)
            {
                await Navigation.PushAsync(new CandidateDashboardPage());
            }
            else
            {
                await Navigation.PushAsync(new CompanyDashboardPage());
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", $"Problem sa prijavom: {ex.Message}", "OK");
        }
    }

    private async void OnResetPasswordClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Reset lozinke", "Link za resetovanje je poslan na vaš email", "OK");
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage(_selectedRole));
    }
}