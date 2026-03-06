using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
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
