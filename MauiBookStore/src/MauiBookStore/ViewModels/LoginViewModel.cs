using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiBookStore.Services.OpenIddict;
using MauiBookStore.Views;
using static Microsoft.Maui.Controls.Shell;

namespace MauiBookStore.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IOpenIddictService _openIddictService;
        [ObservableProperty] private bool _isVisible;
        public LoginViewModel(IOpenIddictService openIddictService) => _openIddictService = openIddictService;


        [RelayCommand]
        public async Task ExecuteLogin()
        {
            IsVisible = false;
            if (await _openIddictService.AuthenticationSuccessful())
            {
                IsVisible = true;
                if (await _openIddictService.IsUserLoggedInAsync())
                {
                    await Current.GoToAsync($"//{nameof(LoginPage)}", false);
                    await Current.GoToAsync($"//{nameof(HomePage)}", false);
                }
                else
                {
                    await _openIddictService.LogoutAsync();
                    await Current.GoToAsync($"//{nameof(LoginPage)}");
                }
            }
            else
            {
                await Current.GoToAsync($"//{nameof(LoginPage)}");
                IsVisible = true;
            }
        }
    }
}