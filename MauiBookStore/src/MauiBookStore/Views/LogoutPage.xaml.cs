using MauiBookStore.ViewModels;

namespace MauiBookStore.Views
{
    public partial class LogoutPage
    {
        public LogoutPage(LogoutViewModel logoutViewModel)
        {
            BindingContext = logoutViewModel;
            InitializeComponent();
        }
    }
}