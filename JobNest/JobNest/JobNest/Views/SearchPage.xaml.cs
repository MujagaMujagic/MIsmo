using JobNest.Models;
using JobNest.Services;
using JobNest.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JobNest.Views;

public partial class SearchPage : ContentPage, INotifyPropertyChanged
{
    private readonly DatabaseService _databaseService;
    private readonly List<JobPost> _allJobs;
    private readonly ObservableCollection<JobPostViewModel> _filteredJobs;

    private string _searchText = "";
    private string _locationFilter = "";
    private string _salaryFilter = "";

    public ObservableCollection<JobPostViewModel> FilteredJobs => _filteredJobs;

    public SearchPage()
    {
        InitializeComponent();
        _databaseService = ServiceHelper.GetService<DatabaseService>();
        _allJobs = new List<JobPost>();
        _filteredJobs = new ObservableCollection<JobPostViewModel>();

        BindingContext = this;
        LoadJobs();
    }

    private async void LoadJobs()
    {
        try
        {
            var jobs = await _databaseService.GetJobPostsAsync();
            _allJobs.Clear();
            _allJobs.AddRange(jobs);

            ApplyFilters();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", $"Problem sa učitavanjem poslova: {ex.Message}", "OK");
        }
    }

    private void ApplyFilters()
    {
        var filtered = _allJobs.AsEnumerable();

        // Search text filter
        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            filtered = filtered.Where(j =>
                j.Title.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                (j.Description?.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        // Location filter
        if (!string.IsNullOrWhiteSpace(_locationFilter))
        {
            filtered = filtered.Where(j =>
                j.Location?.Contains(_locationFilter, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        // Improved salary filter - sada prepoznaje brojeve u opsegu
        if (!string.IsNullOrWhiteSpace(_salaryFilter))
        {
            filtered = filtered.Where(j => MatchesSalaryRange(j.Salary, _salaryFilter));
        }

        var filteredList = filtered.ToList();

        _filteredJobs.Clear();
        foreach (var job in filteredList)
        {
            _filteredJobs.Add(new JobPostViewModel
            {
                Id = job.Id,
                Title = job.Title,
                CompanyName = GetCompanyName(job.CompanyId),
                Description = job.Description,
                Location = job.Location ?? "Lokacija nije navedena",
                Salary = job.Salary ?? "Plata po dogovoru",
                PostedDate = job.PostedDate
            });
        }

        UpdateResultsCount(filteredList.Count);
        UpdateFilterChips();
    }

    private bool MatchesSalaryRange(string? jobSalary, string filterRange)
    {
        if (string.IsNullOrWhiteSpace(jobSalary)) return false;

        // Izvuci brojeve iz job salary (npr. "1500 KM - 2500 KM")
        var salaryNumbers = ExtractNumbers(jobSalary);
        if (!salaryNumbers.Any()) return false;

        // Parsiraj filter range (npr. "1500-2500 KM")
        var rangeParts = filterRange.Split('-');
        if (rangeParts.Length != 2) return false;

        if (int.TryParse(rangeParts[0].Trim(), out int minRange) &&
            int.TryParse(rangeParts[1].Replace("KM", "").Replace("+", "").Trim(), out int maxRange))
        {
            var avgJobSalary = salaryNumbers.Average();

            // Provjeri da li je prosječna plata u opsegu
            if (filterRange.Contains("4000+"))
            {
                return avgJobSalary >= 4000;
            }
            else
            {
                return avgJobSalary >= minRange && avgJobSalary <= maxRange;
            }
        }

        return false;
    }

    private List<int> ExtractNumbers(string text)
    {
        var numbers = new List<int>();
        var parts = text.Split(new char[] { ' ', '-', '–', '—' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            var cleanPart = part.Replace("KM", "").Replace(",", "").Trim();
            if (int.TryParse(cleanPart, out int number))
            {
                numbers.Add(number);
            }
        }

        return numbers;
    }

    private string GetCompanyName(int companyId)
    {
        return companyId switch
        {
            1 => "Tech Solutions BiH",
            2 => "Digital Agency Five",
            3 => "Crvtko Solutions",
            _ => "Kompanija"
        };
    }

    private void UpdateResultsCount(int count)
    {
        ResultsCountLabel.Text = count == 1 ? "1 posao pronađen" : $"{count} poslova pronađeno";
    }

    private void UpdateFilterChips()
    {
        bool hasFilters = !string.IsNullOrWhiteSpace(_locationFilter) ||
                         !string.IsNullOrWhiteSpace(_salaryFilter);

        FilterChipsContainer.IsVisible = hasFilters;

        LocationChip.IsVisible = !string.IsNullOrWhiteSpace(_locationFilter);
        if (LocationChip.IsVisible)
            LocationChipText.Text = _locationFilter;

        SalaryChip.IsVisible = !string.IsNullOrWhiteSpace(_salaryFilter);
        if (SalaryChip.IsVisible)
            SalaryChipText.Text = _salaryFilter;
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        _searchText = e.NewTextValue ?? "";
        ApplyFilters();
    }

    private async void OnFilterClicked(object sender, EventArgs e)
    {
        var action = await DisplayActionSheet("Filteri", "Otkaži", null,
            "Filtriraj po lokaciji", "Filtriraj po plati", "Ukloni sve filtere");

        switch (action)
        {
            case "Filtriraj po lokaciji":
                await ShowLocationFilter();
                break;
            case "Filtriraj po plati":
                await ShowSalaryFilter();
                break;
            case "Ukloni sve filtere":
                ClearAllFilters();
                break;
        }
    }

    private async Task ShowLocationFilter()
    {
        var locations = new[] { "Sarajevo", "Banja Luka", "Tuzla", "Mostar", "Zenica" };
        var action = await DisplayActionSheet("Odaberite lokaciju", "Otkaži", null, locations);

        if (!string.IsNullOrEmpty(action) && action != "Otkaži")
        {
            _locationFilter = action;
            ApplyFilters();
        }
    }

    private async Task ShowSalaryFilter()
    {
        var salaryRanges = new[] { "1000-1500 KM", "1500-2500 KM", "2500-4000 KM", "4000+ KM" };
        var action = await DisplayActionSheet("Odaberite raspon plate", "Otkaži", null, salaryRanges);

        if (!string.IsNullOrEmpty(action) && action != "Otkaži")
        {
            _salaryFilter = action;
            ApplyFilters();
        }
    }

    private void ClearAllFilters()
    {
        _locationFilter = "";
        _salaryFilter = "";
        ApplyFilters();
    }

    private void OnRemoveLocationFilter(object sender, EventArgs e)
    {
        _locationFilter = "";
        ApplyFilters();
    }

    private void OnRemoveSalaryFilter(object sender, EventArgs e)
    {
        _salaryFilter = "";
        ApplyFilters();
    }

    private async void OnJobSelected(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"OnJobSelected called. Selection count: {e.CurrentSelection?.Count ?? 0}");

            if (e.CurrentSelection?.FirstOrDefault() is JobPostViewModel selectedJob)
            {
                System.Diagnostics.Debug.WriteLine($"Selected job: {selectedJob.Title}");

                // Ukloni selekciju odmah da sprječimo duplo klikanje
                ((CollectionView)sender).SelectedItem = null;

                // Navigiraj na SimpleJobDetailPage
                var jobDetailPage = new SimpleJobDetailPage(selectedJob);
                await Navigation.PushAsync(jobDetailPage);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No job selected or wrong type");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnJobSelected: {ex.Message}");
            await DisplayAlert("Greška", $"Problem sa otvaranjem posla: {ex.Message}", "OK");
        }
    }

    private async void OnJobCardTapped(object sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("OnJobCardTapped called");

            if (sender is Frame frame && frame.BindingContext is JobPostViewModel selectedJob)
            {
                System.Diagnostics.Debug.WriteLine($"Job card tapped: {selectedJob.Title}");

                var jobDetailPage = new SimpleJobDetailPage(selectedJob);
                await Navigation.PushAsync(jobDetailPage);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Frame: {sender?.GetType()}, BindingContext: {(sender as Frame)?.BindingContext?.GetType()}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnJobCardTapped: {ex.Message}");
            await DisplayAlert("Greška", $"Problem sa otvaranjem posla: {ex.Message}", "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        base.OnPropertyChanged(propertyName);
    }
}

// ViewModel za prikaz poslova u listi
public class JobPostViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string CompanyName { get; set; } = "";
    public string Description { get; set; } = "";
    public string Location { get; set; } = "";
    public string Salary { get; set; } = "";
    public DateTime PostedDate { get; set; }
}