using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UP_4.Models;

namespace UP_4.ViewModels
{
    public partial class MachineDetailViewModel : ViewModelBase
    {
        private readonly Machine? _editingMachine;

        // Списки для выпадающих списков
        [ObservableProperty] private List<Company> _companies;
        [ObservableProperty] private List<Model> _models;
        [ObservableProperty] private List<Mode> _workModes;
        [ObservableProperty] private List<Place> _places;
        [ObservableProperty] private List<Timezone> _timeZones;
        [ObservableProperty] private List<Priority> _priorities;
        [ObservableProperty] private List<Operator> _operators;
        [ObservableProperty] private List<User> _managers;
        [ObservableProperty] private List<User> _engineers;
        [ObservableProperty] private List<User> _technicians;
        [ObservableProperty] private List<PaymentType> _paymentTypes;
        [ObservableProperty] private List<Mode> _criticalThresholdTemplates;
        [ObservableProperty] private List<Mode> _notificationTemplates;
        [ObservableProperty] private List<Company> _clients; // используем компании как клиентов

        // Поля формы
        [ObservableProperty] private string _name;
        [ObservableProperty] private Company _selectedCompany;
        [ObservableProperty] private Model _selectedModel;
        [ObservableProperty] private Company _selectedSlaveCompany;
        [ObservableProperty] private Model _selectedSlaveModel;
        [ObservableProperty] private Mode _selectedWorkMode;
        [ObservableProperty] private Place _selectedPlace;
        [ObservableProperty] private string _location;
        [ObservableProperty] private string _coordinates;
        [ObservableProperty] private string _serialNumber;
        [ObservableProperty] private string _workingHours;
        [ObservableProperty] private Timezone _selectedTimeZone;
        [ObservableProperty] private Company _selectedClient;
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
        [ObservableProperty] private ObservableCollection<PaymentTypeSelection> _paymentTypeSelections;

        public IAsyncRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public string Title => _editingMachine == null ? "Добавление торгового автомата" : "Редактирование торгового автомата";

        public MachineDetailViewModel(User currentUser, Machine? machineToEdit = null)
        {
            this.currentUser = currentUser;
            _editingMachine = machineToEdit;

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            CancelCommand = new RelayCommand(Cancel);

            LoadDataAsync().ConfigureAwait(false);
        }

        private async Task LoadDataAsync()
        {
            // Загружаем справочники
            Companies = await db.Companys.ToListAsync();
            Models = await db.Models.ToListAsync();
            WorkModes = await db.Modes.ToListAsync();
            Places = await db.Places.ToListAsync();
            TimeZones = await db.Timezones.ToListAsync();
            Priorities = await db.Prioritys.ToListAsync();
            Operators = await db.Operators.ToListAsync();

            Managers = await db.Users.Where(u => u.IsManager == true).ToListAsync();
            Engineers = await db.Users.Where(u => u.IsEngineer == true).ToListAsync();
            Technicians = await db.Users.Where(u => u.IsOperator == true).ToListAsync();

            PaymentTypes = await db.PaymentTypes.ToListAsync();
            PaymentTypeSelections = new ObservableCollection<PaymentTypeSelection>(
                PaymentTypes.Select(pt => new PaymentTypeSelection { PaymentType = pt }));

            CriticalThresholdTemplates = await db.Modes.ToListAsync();
            NotificationTemplates = await db.Modes.ToListAsync();

            Clients = await db.Companys.ToListAsync();

            if (_editingMachine != null)
            {
                // Заполняем поля из редактируемой машины
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

        private async Task SaveAsync()
        {
            try
            {
                // 1. Валидация обязательных полей
                if (string.IsNullOrWhiteSpace(Name) ||
                    SelectedCompany == null ||
                    SelectedModel == null ||
                    SelectedWorkMode == null ||
                    SelectedPlace == null ||
                    string.IsNullOrWhiteSpace(Location) ||
                    string.IsNullOrWhiteSpace(SerialNumber) ||
                    SelectedTimeZone == null ||
                    SelectedPriority == null)
                {
                    // TODO: показать сообщение пользователю (например, через диалог)
                    Console.WriteLine("Ошибка: заполните все обязательные поля.");
                    return;
                }

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

                // 2. Заполняем поля
                machine.Name = Name;
                machine.Company = SelectedCompany.Id;
                machine.Model = SelectedModel.Id;

                // Исправлено: убрано "?", так как выше мы проверили, что SelectedWorkMode != null
                machine.WorkMode = SelectedWorkMode.Id;

                machine.Place = SelectedPlace.Id;
                machine.Location = Location;
                machine.Coordinates = Coordinates;

                if (long.TryParse(SerialNumber, out var sn))
                    machine.SerialNumber = sn;
                else
                    machine.SerialNumber = null; // Или 0, если в БД поле NOT NULL

                machine.WorkingHours = WorkingHours;

                // Исправлено: убрано "?"
                machine.Timezone = SelectedTimeZone.Id;

                // Пользовательские поля
                machine.Manager = SelectedManager?.Id ?? currentUser.Id;
                machine.Engineer = SelectedEngineer?.Id ?? currentUser.Id;
                machine.Technician = SelectedTechnician?.Id ?? currentUser.Id;

                machine.RfidService = ServiceRfid ?? "";
                machine.RfidCashCollection = CollectionRfid ?? "";
                machine.RfidLoading = "";

                // ИСПРАВЛЕНО: Property name 'ServicePriority' instead of 'Priority'
                // Убрано "?", так как есть проверка валидации
                machine.ServicePriority = SelectedPriority.Id;

                machine.KitOnlineId = KitOnlineId ?? "";
                machine.Notes = Notes;

                // Для необязательных полей оставляем ?. (если в БД разрешен NULL)
                machine.CriticalThresholdTemplate = SelectedCriticalThresholdTemplate?.Id;
                machine.NotificationTemplate = SelectedNotificationTemplate?.Id;

                // Дополнительные обязательные поля
                machine.UserId = currentUser.Id;

                if (isNew)
                {
                    machine.InstallDate = DateOnly.FromDateTime(DateTime.Today);
                    machine.LastMaintenanceDate = DateOnly.FromDateTime(DateTime.Today);
                    machine.TotalIncome = 0;
                    machine.Status = 1;
                    // Внимание: убедитесь, что Operator 1 существует, или сделайте поле nullable
                    machine.Operator = 1;
                }

                // 3. Сохранение изменений
                await db.SaveChangesAsync();

                // 4. Обновление платежных систем
                if (!isNew)
                {
                    var oldPayments = await db.MachinePaymentTypes
                        .Where(mpt => mpt.IdMachine == machine.Id)
                        .ToListAsync();
                    db.MachinePaymentTypes.RemoveRange(oldPayments);
                }

                foreach (var selection in PaymentTypeSelections.Where(s => s.IsSelected))
                {
                    db.MachinePaymentTypes.Add(new MachinePaymentType
                    {
                        IdMachine = machine.Id,
                        IdPaymentType = selection.PaymentType.Id
                    });
                }

                await db.SaveChangesAsync();

                // Возврат к списку
                MainWindowViewModel.Instance.CurrentViewModel = new VendingMachinesViewModel(currentUser);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения: {ex.Message}");
                // Можно показать MessageBox здесь
            }
        }

        private void Cancel()
        {
            MainWindowViewModel.Instance.CurrentViewModel = new VendingMachinesViewModel(currentUser);
        }
    }

    public partial class PaymentTypeSelection : ObservableObject
    {
        public PaymentType PaymentType { get; set; }
        [ObservableProperty] private bool _isSelected;
    }
}