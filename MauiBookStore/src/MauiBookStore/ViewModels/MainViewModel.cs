using System.Windows.Input;
using MauiBookStore.Services.OpenIddict;
using MvvmHelpers;
using MvvmHelpers.Commands;

namespace MauiBookStore.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IIdentityService _identityService;

        public MainViewModel(IIdentityService identityService) => _identityService = identityService;

        private string _loginUserMessage, _loginUserName, _loginPassword;
        private AsyncCommand _loginUserCommand;

        public ICommand LoginUserCommand => _loginUserCommand ??=new AsyncCommand(LoginUserAsync);
        private async Task LoginUserAsync()
        {
            LoginUserMessage = await _identityService.LoginAsync(LoginUserName, LoginPassword);
            LoginPassword = null;
            LoginUserName = null;
        }

        public string LoginUserMessage
        {
            get => _loginUserMessage;
            set => SetProperty(ref _loginUserMessage, value);
        }

        public string LoginUserName
        {
            get => _loginUserName;
            set => SetProperty(ref _loginUserName, value);
        }

        public string LoginPassword
        {
            get => _loginPassword;
            set => SetProperty(ref _loginPassword, value);
        }
    }
}