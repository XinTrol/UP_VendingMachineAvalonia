using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using UP_4.Models;
using UP_4.Views;

namespace UP_4.ViewModels
{
    public partial class AuthPageViewModel: ViewModelBase
    {
        [ObservableProperty] string email = "kolbaska321@mail.ru";
        [ObservableProperty] string password = "321";
        [ObservableProperty] string message = "";

        [RelayCommand]
        public void Enter()
        {
            currentUser = db.Users.Include(r => r.IdRoleNavigation).FirstOrDefault(x => x.Email == Email && x.Password == Password);
            if(currentUser == null)
            {
                Message = "Пользователь не найден";
            }
            else
            {
                MainWindowViewModel.Instance.CurrentViewModel = new MainPageViewModel(currentUser);
            }
        }

        [RelayCommand]
        public void Registration()
        {
            MainWindowViewModel.Instance.CurrentViewModel = new RegistrationPageViewModel();
        }
    }
}
