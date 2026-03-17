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

        // Блок 1: Эффективность сети (спидометр)
        [ObservableProperty]
        private double networkEfficiency;

        [ObservableProperty]
        private ObservableCollection<ISeries> gaugeSeries = new();

        // Блок 2: Состояние сети (круговая диаграмма)
        [ObservableProperty]
        private NetworkStatus networkStatus = new();

        [ObservableProperty]
        private ObservableCollection<ISeries> pieSeries = new();

        // Блок 3: Сводка
        [ObservableProperty]
        private SummaryData summaryData = new();

        // Блок 4: Динамика продаж
        [ObservableProperty]
        private ObservableCollection<SalesPoint> salesData = new();

        [ObservableProperty]
        private bool isAmountFilter = true;

        // Блок 5: Новости (заглушка)
        [ObservableProperty]
        private ObservableCollection<NewsItem> news = new();

        // Свойства для графика продаж (столбчатая диаграмма)
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
        private void GoToMachines()
        {
            MainWindowViewModel.Instance.CurrentViewModel = new VendingMachinesViewModel(CurrentUser);
        }

        [RelayCommand]
        private void GoToHome()
        {
            MainWindowViewModel.Instance.CurrentViewModel = new MainPageViewModel(CurrentUser);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                const int workingStatusId = 3;      // Работает
                const int notWorkingStatusId = 2;    // Сломан
                const int maintenanceStatusId = 1;   // Обслуживается

                var machines = await db.Machines.ToListAsync();

                int totalMachines = machines.Count;
                int workingMachines = machines.Count(m => m.Status == workingStatusId);
                NetworkEfficiency = totalMachines > 0 ? (double)workingMachines / totalMachines * 100 : 0;

                // Обновляем NetworkStatus
                NetworkStatus.Working = workingMachines;
                NetworkStatus.NotWorking = machines.Count(m => m.Status == notWorkingStatusId);
                NetworkStatus.UnderMaintenance = machines.Count(m => m.Status == maintenanceStatusId);
                NetworkStatus.Total = totalMachines;

                // Блок 1: Спидометр (кольцо)
                GaugeSeries = new ObservableCollection<ISeries>
                {
                    new PieSeries<double>
                    {
                        Values = new[] { NetworkEfficiency },
                        Fill = new SolidColorPaint(SKColors.Green),
                        DataLabelsSize = 20,
                        DataLabelsPosition = PolarLabelsPosition.End,
                        DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue:F1}%",
                        InnerRadius = 70,   // для создания кольца
                        MaxRadialColumnWidth = 30
                    }
                };

                // Блок 2: Круговая диаграмма
                PieSeries = new ObservableCollection<ISeries>
                {
                    new PieSeries<int>
                    {
                        Name = "Работает",
                        Values = new[] { NetworkStatus.Working },
                        Fill = new SolidColorPaint(SKColors.Green)
                    },
                    new PieSeries<int>
                    {
                        Name = "Не работает",
                        Values = new[] { NetworkStatus.NotWorking },
                        Fill = new SolidColorPaint(SKColors.Red)
                    },
                    new PieSeries<int>
                    {
                        Name = "На обслуживании",
                        Values = new[] { NetworkStatus.UnderMaintenance },
                        Fill = new SolidColorPaint(SKColors.Orange)
                    }
                };

                // Блок 3: Сводка
                decimal totalSales = await db.Sales.SumAsync(s => s.TotalPrice);
                var today = DateTime.Today;
                decimal cashCollection = await db.Sales
                    .Where(s => s.SaleTimestamp.Date == today)
                    .SumAsync(s => s.TotalPrice);

                SummaryData = new SummaryData
                {
                    TotalSales = totalSales,
                    CashCollection = cashCollection,
                    MachinesUnderMaintenance = machines.Count(m => m.Status == maintenanceStatusId)
                };

                // Блок 4: Динамика продаж за 10 дней
                var endDate = DateTime.Today;
                var startDate = endDate.AddDays(-9);

                var salesByDay = await db.Sales
                    .Where(s => s.SaleTimestamp.Date >= startDate && s.SaleTimestamp.Date <= endDate)
                    .GroupBy(s => s.SaleTimestamp.Date)
                    .Select(g => new SalesPoint
                    {
                        Date = g.Key,
                        Amount = g.Sum(s => s.TotalPrice),
                        Quantity = g.Sum(s => s.Quantity)
                    })
                    .OrderBy(s => s.Date)
                    .ToListAsync();

                SalesData.Clear();
                foreach (var item in salesByDay)
                    SalesData.Add(item);

                // После заполнения данных по продажам обновляем график
                UpdateSalesSeries();
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void LoadNewsStub()
        {
            News.Add(new NewsItem
            {
                Title = "Новая акция на кофе",
                Content = "Скидка 20% на все напитки до конца месяца.",
                PublishDate = DateTime.Today.AddDays(-2)
            });
            News.Add(new NewsItem
            {
                Title = "Обновление прошивки",
                Content = "Вышла новая версия ПО для терминалов.",
                PublishDate = DateTime.Today.AddDays(-5)
            });
            News.Add(new NewsItem
            {
                Title = "Вебинар для партнёров",
                Content = "Приглашаем на вебинар по увеличению продаж.",
                PublishDate = DateTime.Today.AddDays(-7)
            });
        }

        // Две отдельные команды для переключения фильтра
        [RelayCommand]
        private void ShowQuantity()
        {
            IsAmountFilter = true;
            UpdateSalesSeries();
        }

        [RelayCommand]
        private void ShowAmount()
        {
            IsAmountFilter = false;
            UpdateSalesSeries();
        }

        // Обновление серий графика продаж
        private void UpdateSalesSeries()
        {
            if (SalesData == null || SalesData.Count == 0)
            {
                // Если данных нет, можно создать тестовые для отладки (раскомментировать при необходимости)
                // SalesData = new ObservableCollection<SalesPoint>(GetTestData());
                SalesSeries = Array.Empty<ISeries>();
                return;
            }

            // Выбираем данные в зависимости от фильтра (количество или сумма)
            var values = IsAmountFilter
                ? SalesData.Select(x => (double)x.Quantity).ToArray()
                : SalesData.Select(x => (double)x.Amount).ToArray();

            SalesSeries = new ObservableCollection<ISeries>
            {
                new ColumnSeries<double>
                {
                    Values = values,
                    Fill = new SolidColorPaint(SKColors.Blue),
                    Stroke = null,
                    Name = IsAmountFilter ? "Количество" : "Сумма, руб"
                }
            };

            // Настройка оси X (даты)
            SalesXAxes = new[]
            {
                new Axis
                {
                    Labels = SalesData.Select(x => x.Date.ToString("dd.MM")).ToArray(),
                    LabelsRotation = 45,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 0.5f }
                }
            };

            // Настройка оси Y (подпись зависит от фильтра)
            SalesYAxes = new[]
            {
                new Axis
                {
                    Name = IsAmountFilter ? "Количество" : "Сумма, руб",
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 0.5f }
                }
            };
        }

        // Вспомогательный метод для тестовых данных
        private List<SalesPoint> GetTestData()
        {
            var list = new List<SalesPoint>();
            var rnd = new Random();
            for (int i = 9; i >= 0; i--)
            {
                list.Add(new SalesPoint
                {
                    Date = DateTime.Today.AddDays(-i),
                    Amount = rnd.Next(500, 2000),
                    Quantity = rnd.Next(5, 30)
                });
            }
            return list;
        }
    }

    // Вспомогательные классы (можно вынести в отдельные файлы)
    public class NetworkStatus : ObservableObject
    {
        private int _working;
        private int _notWorking;
        private int _underMaintenance;
        private int _total;

        public int Working { get => _working; set => SetProperty(ref _working, value); }
        public int NotWorking { get => _notWorking; set => SetProperty(ref _notWorking, value); }
        public int UnderMaintenance { get => _underMaintenance; set => SetProperty(ref _underMaintenance, value); }
        public int Total { get => _total; set => SetProperty(ref _total, value); }
    }

    public class SummaryData : ObservableObject
    {
        private decimal _totalSales;
        private decimal _cashCollection;
        private int _machinesUnderMaintenance;

        public decimal TotalSales { get => _totalSales; set => SetProperty(ref _totalSales, value); }
        public decimal CashCollection { get => _cashCollection; set => SetProperty(ref _cashCollection, value); }
        public int MachinesUnderMaintenance { get => _machinesUnderMaintenance; set => SetProperty(ref _machinesUnderMaintenance, value); }
    }

    public class SalesPoint : ObservableObject
    {
        private DateTime _date;
        private decimal _amount;
        private int _quantity;

        public DateTime Date { get => _date; set => SetProperty(ref _date, value); }
        public decimal Amount { get => _amount; set => SetProperty(ref _amount, value); }
        public int Quantity { get => _quantity; set => SetProperty(ref _quantity, value); }
    }

    public class NewsItem : ObservableObject
    {
        private string _title;
        private string _content;
        private DateTime _publishDate;

        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string Content { get => _content; set => SetProperty(ref _content, value); }
        public DateTime PublishDate { get => _publishDate; set => SetProperty(ref _publishDate, value); }
    }
}