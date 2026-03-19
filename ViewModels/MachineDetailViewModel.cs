using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text; // Для StringBuilder
using System.Threading.Tasks;
using UP_4.Models;

namespace UP_4.ViewModels
{
    public partial class MachineDetailViewModel : ViewModelBase
    {
        private readonly Machine? _editingMachine;

        // Списки для выпадающих списков (инициализируем пустыми, чтобы избежать NullReference)
        [ObservableProperty] private List<Company> _companies = new();
        [ObservableProperty] private List<Model> _models = new();
        [ObservableProperty] private List<Mode> _workModes = new();
        [ObservableProperty] private List<Place> _places = new();
        [ObservableProperty] private List<Timezone> _timeZones = new();
        [ObservableProperty] private List<Priority> _priorities = new();

        [ObservableProperty] private List<User> _managers = new();
        [ObservableProperty] private List<User> _engineers = new();
        [ObservableProperty] private List<User> _technicians = new();

        [ObservableProperty] private List<PaymentType> _paymentTypes = new();
        [ObservableProperty] private List<Mode> _criticalThresholdTemplates = new();
        [ObservableProperty] private List<Mode> _notificationTemplates = new();

        // Поля формы
        [ObservableProperty] private string _name;
        [ObservableProperty] private Company _selectedCompany;
        [ObservableProperty] private Model _selectedModel;
        [ObservableProperty] private Mode _selectedWorkMode;
        [ObservableProperty] private Place _selectedPlace;
        [ObservableProperty] private string _location;
        [ObservableProperty] private string _coordinates;
        [ObservableProperty] private string _serialNumber;
        [ObservableProperty] private string _workingHours;
        [ObservableProperty] private Timezone _selectedTimeZone;

        [ObservableProperty] private User _selectedManager;
        [ObservableProperty] private User _selectedEngineer;
        [ObservableProperty] private User _selectedTechnician;
        [ObservableProperty] private string _serviceRfid;
        [ObservableProperty] private string _collectionRfid;
        [ObservableProperty] private Priority _selectedPriority;
        [ObservableProperty] private string _kitOnlineId;
        [ObservableProperty] private string _notes;
        [ObservableProperty] private Mode _selectedCriticalThresholdTemplate;
        [ObservableProperty] private Mode _selectedNotificationTemplate;
        [ObservableProperty] private ObservableCollection<PaymentTypeSelection> _paymentTypeSelections = new();

        // Ошибки
        [ObservableProperty] private bool _hasError;
        [ObservableProperty] private string _errorMessage;
        [ObservableProperty] private ObservableCollection<string> _validationErrors = new();

        public string Title => _editingMachine == null ? "Добавление торгового автомата" : "Редактирование торгового автомата";

        public MachineDetailViewModel(User currentUser, Machine? machineToEdit = null)
        {
            this.currentUser = currentUser;
            _editingMachine = machineToEdit;

            LoadDataAsync().ConfigureAwait(false);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // Загружаем справочники
                Companies = await db.Companys.ToListAsync();
                Models = await db.Models.ToListAsync();
                WorkModes = await db.Modes.ToListAsync();
                Places = await db.Places.ToListAsync();
                TimeZones = await db.Timezones.ToListAsync();
                Priorities = await db.Prioritys.ToListAsync();

                Managers = await db.Users.Where(u => u.IsManager == true).ToListAsync();
                Engineers = await db.Users.Where(u => u.IsEngineer == true).ToListAsync();
                Technicians = await db.Users.Where(u => u.IsOperator == true).ToListAsync();

                PaymentTypes = await db.PaymentTypes.ToListAsync();
                PaymentTypeSelections = new ObservableCollection<PaymentTypeSelection>(
                    PaymentTypes.Select(pt => new PaymentTypeSelection { PaymentType = pt }));

                CriticalThresholdTemplates = await db.Modes.ToListAsync();
                NotificationTemplates = await db.Modes.ToListAsync();

                if (_editingMachine != null)
                {
                    // Заполняем поля
                    Name = _editingMachine.Name;
                    SelectedCompany = Companies.FirstOrDefault(c => c.Id == _editingMachine.Company);
                    SelectedModel = Models.FirstOrDefault(m => m.Id == _editingMachine.Model);
                    SelectedWorkMode = WorkModes.FirstOrDefault(w => w.Id == _editingMachine.WorkMode);
                    SelectedPlace = Places.FirstOrDefault(p => p.Id == _editingMachine.Place);
                    Location = _editingMachine.Location;
                    Coordinates = _editingMachine.Coordinates;
                    SerialNumber = _editingMachine.SerialNumber?.ToString() ?? "";
                    WorkingHours = _editingMachine.WorkingHours;
                    SelectedTimeZone = TimeZones.FirstOrDefault(t => t.Id == _editingMachine.Timezone);

                    SelectedManager = Managers.FirstOrDefault(u => u.Id == _editingMachine.Manager);
                    SelectedEngineer = Engineers.FirstOrDefault(u => u.Id == _editingMachine.Engineer);
                    SelectedTechnician = Technicians.FirstOrDefault(u => u.Id == _editingMachine.Technician);

                    ServiceRfid = _editingMachine.RfidService;
                    CollectionRfid = _editingMachine.RfidCashCollection;
                    SelectedPriority = Priorities.FirstOrDefault(p => p.Id == _editingMachine.ServicePriority);
                    KitOnlineId = _editingMachine.KitOnlineId;
                    Notes = _editingMachine.Notes;

                    SelectedCriticalThresholdTemplate = CriticalThresholdTemplates.FirstOrDefault(t => t.Id == _editingMachine.CriticalThresholdTemplate);
                    SelectedNotificationTemplate = NotificationTemplates.FirstOrDefault(t => t.Id == _editingMachine.NotificationTemplate);

                    // Загрузка платежных систем
                    var selectedPaymentTypeIds = await db.MachinePaymentTypes
                        .Where(mpt => mpt.IdMachine == _editingMachine.Id)
                        .Select(mpt => mpt.IdPaymentType)
                        .ToListAsync();

                    foreach (var selection in PaymentTypeSelections)
                    {
                        selection.IsSelected = selectedPaymentTypeIds.Contains(selection.PaymentType.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                // Если ошибка при загрузке справочников
                HasError = true;
                ErrorMessage = "Ошибка загрузки данных";
                ValidationErrors = new ObservableCollection<string> { ex.Message };
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            try
            {
                ClearErrors();
                var errors = new List<string>();

                // --- Валидация ---
                if (string.IsNullOrWhiteSpace(Name)) errors.Add("Наименование ТА обязательно.");
                if (SelectedCompany == null) errors.Add("Производитель обязателен.");
                if (SelectedModel == null) errors.Add("Модель обязательна.");
                if (SelectedWorkMode == null) errors.Add("Режим работы обязателен.");
                if (SelectedPlace == null) errors.Add("Место обязательно.");
                if (string.IsNullOrWhiteSpace(Location)) errors.Add("Адрес обязателен.");
                if (string.IsNullOrWhiteSpace(SerialNumber)) errors.Add("Номер автомата обязателен.");
                if (SelectedTimeZone == null) errors.Add("Часовой пояс обязателен.");
                if (SelectedPriority == null) errors.Add("Приоритет обязателен.");
                if (!PaymentTypeSelections.Any(s => s.IsSelected)) errors.Add("Выберите хотя бы одну платежную систему.");

                if (errors.Any())
                {
                    HasError = true;
                    ErrorMessage = "Исправьте ошибки:";
                    ValidationErrors = new ObservableCollection<string>(errors);
                    return;
                }

                // --- Транзакция ---
                await using var transaction = await db.Database.BeginTransactionAsync();

                try
                {
                    Machine machine;
                    bool isNew = _editingMachine == null;

                    if (isNew)
                    {
                        machine = new Machine();
                        db.Machines.Add(machine);
                    }
                    else
                    {
                        machine = _editingMachine;
                    }

                    // Заполнение полей автомата
                    machine.Name = Name;
                    machine.Company = SelectedCompany.Id;
                    machine.Model = SelectedModel.Id;
                    machine.WorkMode = SelectedWorkMode.Id;
                    machine.Place = SelectedPlace.Id;
                    machine.Location = Location;
                    machine.Coordinates = Coordinates;

                    if (long.TryParse(SerialNumber, out var sn)) machine.SerialNumber = sn;
                    else machine.SerialNumber = null;

                    machine.WorkingHours = WorkingHours;
                    machine.Timezone = SelectedTimeZone.Id;

                    machine.Manager = SelectedManager?.Id ?? (isNew ? currentUser.Id : machine.Manager);
                    machine.Engineer = SelectedEngineer?.Id ?? (isNew ? currentUser.Id : machine.Engineer);
                    machine.Technician = SelectedTechnician?.Id ?? (isNew ? currentUser.Id : machine.Technician);

                    machine.RfidService = ServiceRfid ?? "";
                    machine.RfidCashCollection = CollectionRfid ?? "";
                    machine.RfidLoading = "";

                    machine.ServicePriority = SelectedPriority.Id;
                    machine.KitOnlineId = KitOnlineId ?? "";
                    machine.Notes = Notes;

                    machine.CriticalThresholdTemplate = SelectedCriticalThresholdTemplate?.Id;
                    machine.NotificationTemplate = SelectedNotificationTemplate?.Id;
                    machine.UserId = currentUser.Id;

                    if (isNew)
                    {
                        machine.InstallDate = DateOnly.FromDateTime(DateTime.Today);
                        machine.LastMaintenanceDate = DateOnly.FromDateTime(DateTime.Today);
                        machine.TotalIncome = 0;
                        machine.Status = 1;
                        machine.Operator = 1;
                    }

                    // 1. Сохраняем автомат
                    await db.SaveChangesAsync();

                    // 2. Обновление платежных систем
                    var currentMachineId = machine.Id;

                    // Удаляем старые связи
                    var oldPayments = await db.MachinePaymentTypes
                        .Where(mpt => mpt.IdMachine == currentMachineId)
                        .ToListAsync();

                    if (oldPayments.Any())
                    {
                        db.MachinePaymentTypes.RemoveRange(oldPayments);
                        await db.SaveChangesAsync(); // Применяем удаление перед вставкой
                    }

                    // Добавляем новые связи
                    // Здесь Id НЕ указываем, база сама поставит serial
                    foreach (var selection in PaymentTypeSelections.Where(s => s.IsSelected))
                    {
                        db.MachinePaymentTypes.Add(new MachinePaymentType
                        {
                            IdMachine = currentMachineId,
                            IdPaymentType = selection.PaymentType.Id
                        });
                    }

                    // Финальное сохранение
                    await db.SaveChangesAsync();

                    // Подтверждаем транзакцию
                    await transaction.CommitAsync();

                    MainWindowViewModel.Instance.CurrentViewModel = new VendingMachinesViewModel(currentUser);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = "Ошибка сохранения.";

                var errorDetails = new StringBuilder();
                errorDetails.AppendLine(ex.Message);

                var inner = ex.InnerException;
                while (inner != null)
                {
                    errorDetails.AppendLine(inner.Message);
                    inner = inner.InnerException;
                }

                ValidationErrors = new ObservableCollection<string> { errorDetails.ToString() };
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            MainWindowViewModel.Instance.CurrentViewModel = new VendingMachinesViewModel(currentUser);
        }

        [RelayCommand]
        private void NavigateToVendingMachines()
        {
            MainWindowViewModel.Instance.CurrentViewModel = new VendingMachinesViewModel(currentUser);
        }

        [RelayCommand]
        private void ClearErrors()
        {
            HasError = false;
            ErrorMessage = null;
            ValidationErrors.Clear();
        }
    }

    public partial class PaymentTypeSelection : ObservableObject
    {
        public PaymentType PaymentType { get; set; }
        [ObservableProperty] private bool _isSelected;
    }
}