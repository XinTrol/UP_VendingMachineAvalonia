using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using UP_4.Models;

namespace UP_4.ViewModels
{
    public partial class MainPageViewModel: ViewModelBase
    {
        [ObservableProperty] private User currentUser;

        public MainPageViewModel(User user) 
        {
            currentUser = user;                   
        }

    }
}
