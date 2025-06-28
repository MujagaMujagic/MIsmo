using JobNest.Models;

namespace JobNest.Services;

public class CurrentUserService
{
    private User? _currentUser;

    public User? CurrentUser
    {
        get => _currentUser;
        set => _currentUser = value;
    }

    public bool IsLoggedIn => _currentUser != null;

    public void Logout()
    {
        _currentUser = null;
    }

    public string GetGreeting()
    {
        var hour = DateTime.Now.Hour;

        if (hour >= 5 && hour < 12)
            return "Dobro jutro";
        else if (hour >= 12 && hour < 19)
            return "Dobar dan";
        else
            return "Dobro veče";
    }
}