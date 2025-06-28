namespace JobNest.Views;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
        InitializeApp();
    }

    private async void InitializeApp()
    {
        try
        {
            // Wait 2 seconds for splash effect
            await Task.Delay(2000);

            // Navigate to onboarding page
            await Navigation.PushAsync(new OnboardingPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", $"Problem sa pokretanjem: {ex.Message}", "OK");
        }
    }
}