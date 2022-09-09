using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiBookStore.Services.OpenIddict;
using MauiBookStore.Views;
using static Microsoft.Maui.Controls.Shell;

namespace MauiBookStore.ViewModels
{
    public partial class LogoutViewModel : ObservableObject
    {
        private readonly IOpenIddictService _openIddictService;
  
        public LogoutViewModel(IOpenIddictService openIddictService) => _openIddictService = openIddictService;

        [RelayCommand]
        public async Task Logout()
        {
            await _openIddictService.LogoutAsync();
            await Current.GoToAsync($"///{nameof(LoginPage)}");
        }
        
    }
}