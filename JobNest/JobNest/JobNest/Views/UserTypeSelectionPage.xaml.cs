using JobNest.Models;

namespace JobNest.Views;

public partial class UserTypeSelectionPage : ContentPage
{
    public UserTypeSelectionPage()
    {
        InitializeComponent();
    }

    private async void OnCandidateSelected(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LoginPage(UserRole.Candidate));
    }

    private async void OnCompanySelected(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LoginPage(UserRole.Company));
    }
}