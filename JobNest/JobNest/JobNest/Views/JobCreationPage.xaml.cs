using JobNest.Models;
using JobNest.Services;
using JobNest.Helpers;

namespace JobNest.Views;

public partial class JobCreationPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private readonly CurrentUserService _currentUserService;
    private JobPost _editingJob; // Posao koji se uređuje
    private bool _isEditMode; // Da li je ovo uređivanje ili kreiranje novog

    public JobCreationPage()
    {
        InitializeComponent();
        _databaseService = ServiceHelper.GetService<DatabaseService>();
        _currentUserService = ServiceHelper.GetService<CurrentUserService>();
        _isEditMode = false;
    }

    // Konstruktor za uređivanje postojećeg posla
    public JobCreationPage(JobPost existingJob) : this()
    {
        _editingJob = existingJob;
        _isEditMode = true;
        LoadExistingJobData();
    }

    private void LoadExistingJobData()
    {
        if (_editingJob != null)
        {
            // Popuni formu sa postojećim podacima
            JobTitleEntry.Text = _editingJob.Title;
            CategoryPicker.SelectedItem = _editingJob.Category;
            LocationEntry.Text = _editingJob.Location;
            JobTypePicker.SelectedItem = _editingJob.JobType;
            SalaryMinEntry.Text = _editingJob.SalaryMin?.ToString();
            SalaryMaxEntry.Text = _editingJob.SalaryMax?.ToString();
            JobDescriptionEditor.Text = _editingJob.Description;
            RequirementsEditor.Text = _editingJob.Requirements;
            BenefitsEditor.Text = _editingJob.Benefits;

            // Promijeni naslov stranice
            Title = "Uredi posao";

            // Promijeni dugme u "IZBRIŠI POSAO"
            SaveDraftButton.Text = "IZBRIŠI POSAO";
            SaveDraftButton.TextColor = Color.FromArgb("#D32F2F"); // Crvena boja
            SaveDraftButton.BorderColor = Color.FromArgb("#D32F2F");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnPublishJobClicked(object sender, EventArgs e)
    {
        if (!ValidateForm())
            return;

        try
        {
            var currentUser = _currentUserService.CurrentUser;
            if (currentUser == null)
            {
                await DisplayAlert("Greška", "Morate biti ulogovani da biste objavili posao.", "OK");
                return;
            }

            if (_isEditMode && _editingJob != null)
            {
                System.Diagnostics.Debug.WriteLine($"=== POČETAK EDIT MODE ===");
                System.Diagnostics.Debug.WriteLine($"Job ID: {_editingJob.Id}");
                System.Diagnostics.Debug.WriteLine($"SalaryMinEntry.Text: '{SalaryMinEntry.Text}'");
                System.Diagnostics.Debug.WriteLine($"SalaryMaxEntry.Text: '{SalaryMaxEntry.Text}'");
                System.Diagnostics.Debug.WriteLine($"JobTitleEntry.Text: '{JobTitleEntry.Text}'");

                // Provjeri da li su polja prazna PRIJE parsiranja
                if (string.IsNullOrEmpty(SalaryMinEntry.Text) || string.IsNullOrEmpty(SalaryMaxEntry.Text))
                {
                    await DisplayAlert("Greška", "Molimo unesite platu (min i max).", "OK");
                    return;
                }

                // Ažuriraj postojeći posao
                var oldTitle = _editingJob.Title;
                var oldSalaryMin = _editingJob.SalaryMin;
                var oldSalaryMax = _editingJob.SalaryMax;

                // Dodijeli nove vrijednosti sa fallback na stare ako su Entry polja prazna
                _editingJob.Title = !string.IsNullOrWhiteSpace(JobTitleEntry.Text) ? JobTitleEntry.Text.Trim() : _editingJob.Title;
                _editingJob.Description = JobDescriptionEditor.Text?.Trim() ?? _editingJob.Description;
                _editingJob.Category = CategoryPicker.SelectedItem?.ToString() ?? _editingJob.Category;
                _editingJob.Location = !string.IsNullOrWhiteSpace(LocationEntry.Text) ? LocationEntry.Text.Trim() : _editingJob.Location;
                _editingJob.JobType = JobTypePicker.SelectedItem?.ToString() ?? _editingJob.JobType;

                // Parsing plata - ovo je kritično!
                var newSalaryMin = ParseSalary(SalaryMinEntry.Text);
                var newSalaryMax = ParseSalary(SalaryMaxEntry.Text);

                // Samo ažuriraj ako su nove vrijednosti validne (nisu 0)
                if (newSalaryMin > 0) _editingJob.SalaryMin = newSalaryMin;
                if (newSalaryMax > 0) _editingJob.SalaryMax = newSalaryMax;

                _editingJob.Requirements = RequirementsEditor.Text?.Trim() ?? _editingJob.Requirements;
                _editingJob.Benefits = BenefitsEditor.Text?.Trim() ?? _editingJob.Benefits;
                _editingJob.IsActive = true; // Objavljeni posao je aktivan

                System.Diagnostics.Debug.WriteLine($"=== PROMJENE ===");
                System.Diagnostics.Debug.WriteLine($"Title: '{oldTitle}' → '{_editingJob.Title}'");
                System.Diagnostics.Debug.WriteLine($"SalaryMin: {oldSalaryMin} → {_editingJob.SalaryMin}");
                System.Diagnostics.Debug.WriteLine($"SalaryMax: {oldSalaryMax} → {_editingJob.SalaryMax}");

                // Sačuvaj ažuriranja u bazu
                await _databaseService.UpdateJobPostAsync(_editingJob);
                await DisplayAlert("Uspjeh", "Posao je uspješno ažuriran!", "OK");
            }
            else
            {
                // Kreiraj novi posao
                var newJob = new JobPost
                {
                    Title = JobTitleEntry.Text?.Trim(),
                    Description = JobDescriptionEditor.Text?.Trim(),
                    Category = CategoryPicker.SelectedItem?.ToString(),
                    Location = LocationEntry.Text?.Trim(),
                    JobType = JobTypePicker.SelectedItem?.ToString(),
                    SalaryMin = ParseSalary(SalaryMinEntry.Text),
                    SalaryMax = ParseSalary(SalaryMaxEntry.Text),
                    Requirements = RequirementsEditor.Text?.Trim(),
                    Benefits = BenefitsEditor.Text?.Trim(),
                    CompanyId = currentUser.Id,
                    Company = currentUser.Name,
                    PostedDate = DateTime.Now,
                    IsActive = true
                };

                // Sačuvaj u bazu
                await _databaseService.CreateJobPostAsync(newJob);
                await DisplayAlert("Uspjeh", "Posao je uspješno objavljen!", "OK");
            }

            // Vrati se na company dashboard
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", $"Greška pri objavljivanju posla: {ex.Message}", "OK");
        }
    }

    private async void OnSaveDraftClicked(object sender, EventArgs e)
    {
        try
        {
            var currentUser = _currentUserService.CurrentUser;
            if (currentUser == null)
            {
                await DisplayAlert("Greška", "Morate biti ulogovani.", "OK");
                return;
            }

            if (_isEditMode && _editingJob != null)
            {
                // U edit mode-u, ovo dugme briše posao
                var confirm = await DisplayAlert("Potvrda",
                    $"Da li ste sigurni da želite obrisati posao '{_editingJob.Title}'?\n\nOvo će takođe obrisati sve aplikacije za ovaj posao.",
                    "Obriši", "Otkaži");

                if (confirm)
                {
                    // Obriši posao (soft delete)
                    _editingJob.IsActive = false;
                    _editingJob.Title = "[OBRISAN] " + _editingJob.Title;
                    await _databaseService.UpdateJobPostAsync(_editingJob);

                    await DisplayAlert("Uspjeh", "Posao je uspješno obrisan!", "OK");
                    await Navigation.PopAsync(); // Vrati se na dashboard
                    return;
                }
                else
                {
                    return; // Otkazano brisanje
                }
            }
            else
            {
                // Kreiraj novi draft
                var draftJob = new JobPost
                {
                    Title = JobTitleEntry.Text?.Trim() ?? "Draft",
                    Description = JobDescriptionEditor.Text?.Trim(),
                    Category = CategoryPicker.SelectedItem?.ToString(),
                    Location = LocationEntry.Text?.Trim(),
                    JobType = JobTypePicker.SelectedItem?.ToString(),
                    SalaryMin = ParseSalary(SalaryMinEntry.Text),
                    SalaryMax = ParseSalary(SalaryMaxEntry.Text),
                    Requirements = RequirementsEditor.Text?.Trim(),
                    Benefits = BenefitsEditor.Text?.Trim(),
                    CompanyId = currentUser.Id,
                    Company = currentUser.Name,
                    PostedDate = DateTime.Now,
                    IsActive = false // Draft nije aktivan
                };

                await _databaseService.CreateJobPostAsync(draftJob);
                await DisplayAlert("Uspjeh", "Draft je sačuvan.", "OK");
            }

            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", $"Greška pri čuvanju draft-a: {ex.Message}", "OK");
        }
    }

    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(JobTitleEntry.Text))
        {
            DisplayAlert("Greška", "Naziv pozicije je obavezan.", "OK");
            return false;
        }

        if (CategoryPicker.SelectedItem == null)
        {
            DisplayAlert("Greška", "Molimo odaberite kategoriju.", "OK");
            return false;
        }

        if (string.IsNullOrWhiteSpace(LocationEntry.Text))
        {
            DisplayAlert("Greška", "Lokacija je obavezna.", "OK");
            return false;
        }

        if (JobTypePicker.SelectedItem == null)
        {
            DisplayAlert("Greška", "Molimo odaberite tip posla.", "OK");
            return false;
        }

        if (string.IsNullOrWhiteSpace(SalaryMinEntry.Text) || string.IsNullOrWhiteSpace(SalaryMaxEntry.Text))
        {
            DisplayAlert("Greška", "Molimo unesite platu (od - do).", "OK");
            return false;
        }

        if (string.IsNullOrWhiteSpace(JobDescriptionEditor.Text))
        {
            DisplayAlert("Greška", "Opis posla je obavezan.", "OK");
            return false;
        }

        return true;
    }

    private decimal ParseSalary(string salaryText)
    {
        System.Diagnostics.Debug.WriteLine($"ParseSalary dobio: '{salaryText}'");

        if (string.IsNullOrWhiteSpace(salaryText))
        {
            System.Diagnostics.Debug.WriteLine("ParseSalary: Prazan string, vraćam 0");
            return 0;
        }

        if (decimal.TryParse(salaryText, out decimal salary))
        {
            System.Diagnostics.Debug.WriteLine($"ParseSalary: Uspješno parsiran = {salary}");
            return salary;
        }

        System.Diagnostics.Debug.WriteLine($"ParseSalary: NEUSPJEŠNO parsiranje '{salaryText}', vraćam 0");
        return 0;
    }
}