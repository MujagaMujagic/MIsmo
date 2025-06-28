using JobNest.Views;

namespace JobNest;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Registruj rute za navigaciju
        Routing.RegisterRoute("splash", typeof(SplashPage));
        Routing.RegisterRoute("welcome", typeof(WelcomePage));
    }
}