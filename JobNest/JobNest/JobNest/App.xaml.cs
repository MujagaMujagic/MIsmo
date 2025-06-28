using JobNest.Services;
using JobNest.Views;

namespace JobNest;

public partial class App : Application
{
    private readonly DatabaseService _databaseService;

    public App(DatabaseService databaseService)
    {
        InitializeComponent();

        _databaseService = databaseService;

        // Kreiraj bazu pri pokretanju
        Task.Run(async () =>
        {
            await _databaseService.InitializeDatabaseAsync();
        });

        MainPage = new NavigationPage(new SplashPage());
    }
}