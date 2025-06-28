using Microsoft.Extensions.DependencyInjection;

namespace JobNest.Helpers;

public static class ServiceHelper
{
    public static TService GetService<TService>() where TService : class
        => Current.GetService<TService>();

    public static IServiceProvider Current =>
#if WINDOWS10_0_17763_0_OR_GREATER
        MauiWinUIApplication.Current.Services;
#elif ANDROID
        IPlatformApplication.Current.Services;
#elif IOS || MACCATALYST
        IPlatformApplication.Current.Services;
#else
        IPlatformApplication.Current?.Services;
#endif
}