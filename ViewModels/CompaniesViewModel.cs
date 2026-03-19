using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UP_4.Models;
using Avalonia.Platform.Storage;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia;

namespace UP_4.ViewModels
{
    public partial class CompaniesViewModel : ViewModelBase
    {
        [ObservableProperty]
        private User currentUser;  // Добавлено

        [ObservableProperty] private string searchText = "";
        [ObservableProperty] private int currentPage = 1;
        [ObservableProperty] private int itemsPerPage = 10;
        [ObservableProperty] private int totalItems;

        [ObservableProperty] private bool isTableView = true;
        [ObservableProperty] private bool isTileView = false;

        private List<Company> _allCompanies = new();

        [ObservableProperty]
        private ObservableCollection<Company> displayedCompanies = new();

        public List<int> ItemsPerPageOptions { get; } = new() { 5, 10, 25, 50, 100 };

        public string ItemsInfo => $"Показано {DisplayedCompanies.Count} из {TotalItems}";
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / ItemsPerPage);

        // Конструктор с параметром User
        public CompaniesViewModel(User user)
        {
            CurrentUser = user;
            LoadDataCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadData()
        {
            _allCompanies = await db.Companys
                .Include(c => c.ParentCompany)
                .ToListAsync();

            Refresh();
        }

        partial void OnSearchTextChanged(string value) => Refresh();
        partial void OnItemsPerPageChanged(int value) => Refresh();
        partial void OnCurrentPageChanged(int value) => Refresh();

        private void Refresh()
        {
            var query = _allCompanies.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(c =>
                    c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            TotalItems = query.Count();

            if (CurrentPage > TotalPages && TotalPages > 0)
                CurrentPage = TotalPages;
            if (CurrentPage < 1) CurrentPage = 1;

            var items = query
                .Skip((CurrentPage - 1) * ItemsPerPage)
                .Take(ItemsPerPage)
                .ToList();

            DisplayedCompanies.Clear();
            foreach (var item in items)
                DisplayedCompanies.Add(item);

            OnPropertyChanged(nameof(ItemsInfo));
            OnPropertyChanged(nameof(TotalPages));
        }

        [RelayCommand]
        private async Task DeleteCompany(Company company)
        {
            if (company == null) return;

            db.Companys.Remove(company);
            await db.SaveChangesAsync();

            _allCompanies.Remove(company);
            Refresh();
        }

        [RelayCommand]
        private void EditCompany(Company company)
        {
            MainWindowViewModel.Instance.CurrentViewModel =
                new CompanyEditViewModel(company, CurrentUser);
        }

        [RelayCommand]
        private void AddCompany()
        {
            MainWindowViewModel.Instance.CurrentViewModel =
                new CompanyEditViewModel(null, CurrentUser);
        }

        [RelayCommand]
        private async Task ExportCsv()
        {
            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
                return;

            var window = desktop.MainWindow;
            if (window == null) return;

            var storage = window.StorageProvider;
            var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Экспорт компаний",
                SuggestedFileName = "companies.csv",
                FileTypeChoices = new[] { new FilePickerFileType("CSV") { Patterns = new[] { "*.csv" } } }
            });

            if (file != null)
            {
                var csv = new StringBuilder();
                csv.AppendLine("Название;Вышестоящая;Адрес;Телефон;Email;Дата создания;ИНН");
                
                foreach (var c in DisplayedCompanies)
                {
                    var parentName = c.ParentCompany?.Name ?? "";
                    var created = c.CreatedDate.ToString("dd.MM.yyyy");
                    csv.AppendLine($"{c.Name};{parentName};{c.Address};{c.Phone};{c.Email};{created};{c.Inn}");
                }

                await using var stream = await file.OpenWriteAsync();
                await using var writer = new StreamWriter(stream, Encoding.UTF8);
                await writer.WriteAsync(csv.ToString());
            }
        }

        [RelayCommand]
        private void NextPage()
        {
            if (CurrentPage < TotalPages) CurrentPage++;
        }

        [RelayCommand]
        private void PrevPage()
        {
            if (CurrentPage > 1) CurrentPage--;
        }

        [RelayCommand]
        private void GoToMachines()
        {
            MainWindowViewModel.Instance.CurrentViewModel = new VendingMachinesViewModel(CurrentUser);
        }

        [RelayCommand]
        private void GoToHome()
        {
            MainWindowViewModel.Instance.CurrentViewModel = new MainPageViewModel(CurrentUser);
        }

        [RelayCommand]
        private void GoToCompanies()
        {
            MainWindowViewModel.Instance.CurrentViewModel = new CompaniesViewModel(CurrentUser);
        }
    }
}