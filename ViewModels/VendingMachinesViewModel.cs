using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using UP_4.Models;

namespace UP_4.ViewModels
{
    public partial class VendingMachinesViewModel : ViewModelBase
    {
        [ObservableProperty] private bool _isTableView = true;
        [ObservableProperty] private string _searchText = string.Empty;
        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private int _itemsPerPage = 10;
        [ObservableProperty] private int _totalItems;

        private List<Machine> _allMachinesCache = new();
        [ObservableProperty]
        private ObservableCollection<Machine> _displayedMachines = new();

        public List<int> ItemsPerPageOptions { get; } = new() { 5, 10, 25, 50, 100 };
        public string ItemsInfo => $"Показано {DisplayedMachines.Count} из {TotalItems}";
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / ItemsPerPage);

        public VendingMachinesViewModel()
        {
            LoadDataCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadData()
        {
            _allMachinesCache = await db.Machines
                .Include(m => m.ModelNavigation)
                .Include(m => m.CompanyNavigation)
                .Include(m => m.PlaceNavigation)
                .ToListAsync();
            RefreshData();
        }

        partial void OnSearchTextChanged(string value) => RefreshData();
        partial void OnItemsPerPageChanged(int value) => RefreshData();
        partial void OnCurrentPageChanged(int value) => RefreshData();

        private void RefreshData()
        {
            var query = _allMachinesCache.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(m =>
                    m.Name != null &&
                    m.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            TotalItems = query.Count();

            if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;
            if (CurrentPage < 1) CurrentPage = 1;

            var itemsToDisplay = query
                .Skip((CurrentPage - 1) * ItemsPerPage)
                .Take(ItemsPerPage)
                .ToList();

            DisplayedMachines.Clear();
            foreach (var item in itemsToDisplay)
            {
                DisplayedMachines.Add(item);
            }

            OnPropertyChanged(nameof(ItemsInfo));
            OnPropertyChanged(nameof(TotalPages));
        }

        [RelayCommand]
        private void ToggleView(string viewMode)
        {
            IsTableView = viewMode == "Table";
        }

        [RelayCommand]
        private async Task ExportCsv()
        {
            if (App.Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;
            var window = desktop.MainWindow;
            if (window == null) return;

            var storage = window.StorageProvider;
            var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Экспорт данных",
                SuggestedFileName = "vending_machines.csv",
                FileTypeChoices = new[] { new FilePickerFileType("CSV") { Patterns = new[] { "*.csv" } } }
            });

            if (file != null)
            {
                var csv = new StringBuilder();
                csv.AppendLine("ID;Название;Модель;Компания;Модем;Адрес;В работе с");
                foreach (var m in DisplayedMachines)
                {
                    csv.AppendLine($"{m.Id};{m.Name};{m.ModelNavigation?.Name};{m.CompanyNavigation?.Name};{m.KitOnlineId};{m.PlaceNavigation?.Name};{m.InstallDate:dd.MM.yyyy}");
                }
                await using var stream = await file.OpenWriteAsync();
                await using var writer = new StreamWriter(stream, Encoding.UTF8);
                await writer.WriteAsync(csv.ToString());
            }
        }

        // ИСПРАВЛЕНО: Названия методов совпадают с CommandName в XAML
        [RelayCommand]
        private async Task DeleteMachine(Machine machine)
        {
            if (machine == null) return;

            db.Machines.Remove(machine);
            await db.SaveChangesAsync();

            _allMachinesCache.Remove(machine);
            RefreshData();
        }

        [RelayCommand]
        private async Task UnlinkModem(Machine machine)
        {
            if (machine == null) return;
            machine.KitOnlineId = "Отвязан";
            db.Update(machine);
            await db.SaveChangesAsync();
            RefreshData();
        }

        [RelayCommand]
        private void EditMachine(Machine machine)
        {
            // Логика редактирования
        }

        [RelayCommand]
        private void NextPage() { if (CurrentPage < TotalPages) CurrentPage++; }

        [RelayCommand]
        private void PrevPage() { if (CurrentPage > 1) CurrentPage--; }
    }
}