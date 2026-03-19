using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UP_4.Models;

namespace UP_4.ViewModels
{
    public partial class CompanyEditViewModel : ViewModelBase
    {
        private readonly Company _company;
        private readonly bool _isNew;

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private string _address;

        [ObservableProperty]
        private string _phone;

        [ObservableProperty]
        private string _email;

        [ObservableProperty]
        private string _inn;

        [ObservableProperty]
        private ObservableCollection<Company> _companies = new();

        [ObservableProperty]
        private Company _selectedParentCompany;

        public string Title => _isNew ? "Добавление компании" : "Редактирование компании";

        public CompanyEditViewModel(Company? company, User currentUser)
        {
            this.currentUser = currentUser;
            _isNew = company == null;
            _company = company ?? new Company();

            if (_isNew)
            {
                // Для новой компании устанавливаем текущую дату (тип DateOnly)
                _company.CreatedDate = DateOnly.FromDateTime(DateTime.Today);
            }
            else
            {
                // Заполняем поля из существующей компании
                Name = _company.Name;
                Address = _company.Address;
                Phone = _company.Phone;
                Email = _company.Email;
                Inn = _company.Inn;
            }

            LoadCompaniesAsync().ConfigureAwait(false);
        }

        private async Task LoadCompaniesAsync()
        {
            try
            {
                var allCompanies = await db.Companys.ToListAsync();
                Companies = new ObservableCollection<Company>(
                    allCompanies.Where(c => c.Id != _company.Id)
                );

                if (!_isNew && _company.ParentCompanyId.HasValue)
                {
                    SelectedParentCompany = Companies.FirstOrDefault(c => c.Id == _company.ParentCompanyId.Value);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки компаний: {ex.Message}");
                // TODO: показать диалог с ошибкой (например, MessageBox.Avalonia)
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            // 1. Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(Name))
            {
                await ShowError("Название компании обязательно.");
                return;
            }

            // Если в базе ИНН может быть NULL, эту проверку можно убрать, 
            // но лучше оставить, если бизнес-логика требует ИНН.
            if (string.IsNullOrWhiteSpace(Inn))
            {
                await ShowError("ИНН обязателен.");
                return;
            }

            try
            {
                // 2. Заполняем свойства сущности
                _company.Name = Name;
                _company.Address = Address;
                _company.Phone = Phone;
                _company.Email = Email;
                _company.Inn = Inn;

                // Важно: присваиваем ID выбранного родителя, а не саму сущность
                _company.ParentCompanyId = SelectedParentCompany?.Id;

                // 3. Логика сохранения
                if (_isNew)
                {
                    // Добавление новой записи
                    db.Companys.Add(_company);
                }
                else
                {
                    // --- РЕШЕНИЕ ПРОБЛЕМЫ UPDATE ---

                    // Если контекст долгоживущий, сущность может быть уже загружена (tracked).
                    // Пробуем получить текущий отслеживаемый экземпляр.
                    var trackedEntity = db.ChangeTracker.Entries<Company>()
                        .FirstOrDefault(e => e.Entity.Id == _company.Id);

                    if (trackedEntity != null)
                    {
                        // Если сущность уже отслеживается, обновляем её значения прямо в трекере.
                        // Это нужно, если мы передали в VM "старую" копию объекта, а в контексте она уже есть.
                        trackedEntity.CurrentValues.SetValues(_company);
                        trackedEntity.State = EntityState.Modified;
                    }
                    else
                    {
                        // Если сущность не отслеживается (была Detach или контекст новый), просто аттачим.
                        db.Companys.Attach(_company);
                        db.Entry(_company).State = EntityState.Modified;
                    }
                }

                // 4. Сохраняем изменения в БД
                await db.SaveChangesAsync();

                // 5. Возвращаемся назад
                MainWindowViewModel.Instance.CurrentViewModel = new CompaniesViewModel(currentUser);
            }
            catch (Exception ex)
            {
                // Перехватываем ошибку уникальности ИНН или другие ошибки БД
                if (ex.InnerException?.Message.Contains("unique_inn") == true || ex.Message.Contains("unique_inn"))
                {
                    await ShowError("Компания с таким ИНН уже существует.");
                }
                else
                {
                    await ShowError($"Ошибка сохранения:\n{ex.InnerException?.Message ?? ex.Message}");
                }
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            MainWindowViewModel.Instance.CurrentViewModel = new CompaniesViewModel(currentUser);
        }

        // Вспомогательный метод для отображения ошибок
        private async Task ShowError(string message)
        {
            // Здесь можно использовать MessageBox (например, из MessageBox.Avalonia)
            // Если пакет не установлен, временно выводим в Debug и показываем простой диалог
            System.Diagnostics.Debug.WriteLine(message);
            // TODO: заменить на реальный диалог
            // await MessageBox.Avalonia.MessageBoxManager
            //     .GetMessageBoxStandardWindow("Ошибка", message)
            //     .Show();
        }
    }
}