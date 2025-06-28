using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using JobNest.Data;
using JobNest.Services;
using CommunityToolkit.Maui;

namespace JobNest;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Database konfiguracija
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={FileSystem.AppDataDirectory}/jobnest.db"));

        // Registruj servise
        builder.Services.AddScoped<DatabaseService>();
        builder.Services.AddSingleton<CurrentUserService>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}