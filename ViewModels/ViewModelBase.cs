using CommunityToolkit.Mvvm.ComponentModel;
using UP_4.Models;

namespace UP_4.ViewModels
{
    public class ViewModelBase : ObservableObject
    {
        public SavukovContext db = new SavukovContext();
        public User currentUser;
    }
}
