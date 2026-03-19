using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UP_4.Models;

namespace UP_4.ViewModels
{
    public partial class MainPageViewModel : ViewModelBase
    {
        [ObservableProperty]
        private User currentUser;

        // Блок 1
        [ObservableProperty]
        private double networkEfficiency;
        [ObservableProperty]
        private ObservableCollection<ISeries> gaugeSeries = new();

        // Блок 2
        [ObservableProperty]
        private NetworkStatus networkStatus = new();
        [ObservableProperty]
        private ObservableCollection<ISeries> pieSeries = new();

        // Блок 3
        [ObservableProperty]
        private SummaryData summaryData = new();

        // Блок 4
        [ObservableProperty]
        private ObservableCollection<SalesPoint> salesData = new();
        [ObservableProperty]
        private bool isAmountFilter = true;

        // Блок 5
        [ObservableProperty]
        private ObservableCollection<NewsItem> news = new();

        // Графики
        [ObservableProperty]
        private IEnumerable<ISeries> salesSeries;
        [ObservableProperty]
        private Axis[] salesXAxes;
        [ObservableProperty]
        private Axis[] salesYAxes;

        public IAsyncRelayCommand LoadDataCommand { get; }

        public MainPageViewModel(User user)
        {
            CurrentUser = user;
            LoadNewsStub();
            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
            LoadDataCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        private void GoToMachines() => MainWindowViewModel.Instance.CurrentViewModel = new VendingMachinesViewModel(CurrentUser);

        [RelayCommand]
        private void GoToHome() => MainWindowViewModel.Instance.CurrentViewModel = new MainPageViewModel(CurrentUser);

        [RelayCommand]
        private void GoToCompanies() => MainWindowViewModel.Instance.CurrentViewModel = new CompaniesViewModel(CurrentUser);

        private async Task LoadDataAsync()
        {
            try
            {
                const int workingStatusId = 3;
                const int notWorkingStatusId = 2;
                const int maintenanceStatusId = 1;

                var machines = await db.Machines.ToListAsync();
                int totalMachines = machines.Count;
                int workingMachines = machines.Count(m => m.Status == workingStatusId);

                NetworkEfficiency = totalMachines > 0 ? (double)workingMachines / totalMachines * 100 : 0;

                NetworkStatus.Working = workingMachines;
                NetworkStatus.NotWorking = machines.Count(m => m.Status == notWorkingStatusId);
                NetworkStatus.UnderMaintenance = machines.Count(m => m.Status == maintenanceStatusId);
                NetworkStatus.Total = totalMachines;

                GaugeSeries = new ObservableCollection<ISeries>
                {
                    new PieSeries<double>
                    {
                        Values = new[] { NetworkEfficiency },
                        Fill = new SolidColorPaint(SKColors.Green),
                        DataLabelsSize = 20,
                        DataLabelsPosition = PolarLabelsPosition.End,
                        DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue:F1}%",
                        InnerRadius = 70,
                        MaxRadialColumnWidth = 30
                    }
                };

                PieSeries = new ObservableCollection<ISeries>
                {
                    new PieSeries<int> { Name = "Работает", Values = new[] { NetworkStatus.Working }, Fill = new SolidColorPaint(SKColors.Green) },
                    new PieSeries<int> { Name = "Не работает", Values = new[] { NetworkStatus.NotWorking }, Fill = new SolidColorPaint(SKColors.Red) },
                    new PieSeries<int> { Name = "На обслуживании", Values = new[] { NetworkStatus.UnderMaintenance }, Fill = new SolidColorPaint(SKColors.Orange) }
                };

                decimal totalSales = await db.Sales.SumAsync(s => s.TotalPrice);
                var today = DateTime.Today;
                decimal cashCollection = await db.Sales
                    .Where(s => s.SaleTimestamp >= today && s.SaleTimestamp < today.AddDays(1))
                    .SumAsync(s => s.TotalPrice);

                SummaryData = new SummaryData
                {
                    TotalSales = totalSales,
                    CashCollection = cashCollection,
                    MachinesUnderMaintenance = machines.Count(m => m.Status == maintenanceStatusId)
                };

                // Загрузка графика
                await LoadSalesDataAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки: {ex.Message}");
            }
        }

        private async Task LoadSalesDataAsync()
        {
            // Используем "безопасный" способ, как у друга
            var tempSalesData = new List<SalesPoint>();

            try
            {
                for (int i = 9; i >= 0; i--)
                {
                    var date = DateTime.Today.AddDays(-i);

                    // Фильтр: начало дня и конец дня
                    var dayStart = date;
                    var dayEnd = date.AddDays(1);

                    // Запрос к БД
                    var daySales = await db.Sales
                        .Where(s => s.SaleTimestamp >= dayStart && s.SaleTimestamp < dayEnd)
                        .ToListAsync();

                    tempSalesData.Add(new SalesPoint
                    {
                        Date = date,
                        Amount = daySales.Sum(s => s.TotalPrice),
                        Quantity = daySales.Sum(s => s.Quantity)
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при чтении продаж: {ex.Message}");
                // Если ошибка, заполняем нулями, чтобы не падало
                for (int i = 9; i >= 0; i--)
                {
                    tempSalesData.Add(new SalesPoint { Date = DateTime.Today.AddDays(-i), Amount = 0, Quantity = 0 });
                }
            }

            SalesData = new ObservableCollection<SalesPoint>(tempSalesData);
            UpdateSalesSeries();
        }

        private void LoadNewsStub()
        {
            News.Add(new NewsItem { Title = "Новая акция", Content = "Скидки", PublishDate = DateTime.Today });
        }

        [RelayCommand]
        private void ShowQuantity() { IsAmountFilter = true; UpdateSalesSeries(); }

        [RelayCommand]
        private void ShowAmount() { IsAmountFilter = false; UpdateSalesSeries(); }

        private void UpdateSalesSeries()
        {
            if (SalesData == null || SalesData.Count == 0) return;

            var values = IsAmountFilter
                ? SalesData.Select(x => (double)x.Quantity).ToArray()
                : SalesData.Select(x => (double)x.Amount).ToArray();

            SalesSeries = new ObservableCollection<ISeries>
            {
                new ColumnSeries<double>
                {
                    Values = values,
                    Fill = new SolidColorPaint(SKColors.DodgerBlue),
                    Name = IsAmountFilter ? "Количество" : "Сумма"
                }
            };

            SalesXAxes = new[]
            {
                new Axis
                {
                    Labels = SalesData.Select(x => x.Date.ToString("dd.MM")).ToArray(),
                    LabelsRotation = 45
                }
            };

            SalesYAxes = new[]
            {
                new Axis
                {
                    Name = IsAmountFilter ? "Шт." : "Руб.",
                    MinLimit = 0
                }
            };
        }
    }

    // Вспомогательные классы
    public class NetworkStatus : ObservableObject
    {
        public int Working { get; set; }
        public int NotWorking { get; set; }
        public int UnderMaintenance { get; set; }
        public int Total { get; set; }
    }
    public class SummaryData : ObservableObject
    {
        public decimal TotalSales { get; set; }
        public decimal CashCollection { get; set; }
        public int MachinesUnderMaintenance { get; set; }
    }
    public class SalesPoint
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public int Quantity { get; set; }
    }
    public class NewsItem
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime PublishDate { get; set; }
    }
}