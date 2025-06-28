using JobNest.Models;
using JobNest.Services;
using JobNest.Helpers;
using System.Linq;

namespace JobNest.Views;

public partial class RegisterPage : ContentPage
{
    private bool _isPasswordVisible = false;
    private UserRole _selectedRole = UserRole.Candidate;
    private readonly DatabaseService _databaseService;

    public RegisterPage()
    {
        InitializeComponent();
        _databaseService = ServiceHelper.GetService<DatabaseService>();
        SetupUI(UserRole.Candidate);
    }

    public RegisterPage(UserRole userRole)
    {
        InitializeComponent();
        _databaseService = ServiceHelper.GetService<DatabaseService>();
        SetupUI(userRole);
    }

    private void SetupUI(UserRole userRole)
    {
        _selectedRole = userRole;

        if (_selectedRole == UserRole.Candidate)
        {
            RoleLabel.Text = "Kandidat";
            CompanyFields.IsVisible = false;
        }
        else
        {
            RoleLabel.Text = "Kompanija";
            CompanyFields.IsVisible = true;
        }
    }

    private void OnShowPasswordClicked(object sender, EventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        PasswordEntry.IsPassword = !_isPasswordVisible;
        ShowPasswordButton.Text = _isPasswordVisible ? "🙈" : "👁";
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        // Validacija imena i prezimena
        if (string.IsNullOrEmpty(FirstNameEntry.Text))
        {
            await DisplayAlert("Greška", "Molimo unesite ime", "OK");
            return;
        }

        if (string.IsNullOrEmpty(LastNameEntry.Text))
        {
            await DisplayAlert("Greška", "Molimo unesite prezime", "OK");
            return;
        }

        if (string.IsNullOrEmpty(EmailEntry.Text) || !IsValidEmail(EmailEntry.Text))
        {
            await DisplayAlert("Greška", "Molimo unesite valjan email", "OK");
            return;
        }

        if (!IsValidPassword(PasswordEntry.Text))
        {
            await DisplayAlert("Greška", "Lozinka mora imati minimum 8 karaktera sa velikim i malim slovima, brojem i specijalnim znakom", "OK");
            return;
        }

        if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            await DisplayAlert("Greška", "Lozinke se ne poklapaju", "OK");
            return;
        }

        if (!TermsCheckBox.IsChecked)
        {
            await DisplayAlert("Greška", "Morate se složiti sa uslovima korištenja", "OK");
            return;
        }

        // Validacija za kompaniju
        if (_selectedRole == UserRole.Company && string.IsNullOrEmpty(CompanyNameEntry.Text))
        {
            await DisplayAlert("Greška", "Molimo unesite naziv kompanije", "OK");
            return;
        }

        try
        {
            
            // Kreiranje korisnika u bazi
            string firstName = FirstNameEntry.Text;
            string lastName = LastNameEntry.Text;
            string companyName = _selectedRole == UserRole.Company ? CompanyNameEntry.Text : "";

            
            var user = await _databaseService.CreateUserAsync(
                EmailEntry.Text,
                PasswordEntry.Text,
                _selectedRole,
                firstName,
                lastName,
                companyName);

            string fullName = $"{firstName} {lastName}";
            string roleText = _selectedRole == UserRole.Candidate ? "kandidat" : "kompanija";
            await DisplayAlert("Uspjeh", $"Dobrodošli {fullName}! Uspješno ste kreirali račun kao {roleText}!", "OK");

            // Vrati na login stranicu
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", $"Problem sa registracijom: {ex.Message}\n\nStack trace: {ex.StackTrace}", "OK");
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidPassword(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8)
            return false;

        bool hasUpper = password.Any(char.IsUpper);
        bool hasLower = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

        return hasUpper && hasLower && hasDigit && hasSpecial;
    }
}