using CommunityToolkit.Mvvm.ComponentModel;

namespace UP_4.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public static MainWindowViewModel Instance { get; set; }
        
        [ObservableProperty]
        private ViewModelBase currentViewModel = new AuthPageViewModel();

        public MainWindowViewModel()
        {
            Instance = this;
        }
    }
}
