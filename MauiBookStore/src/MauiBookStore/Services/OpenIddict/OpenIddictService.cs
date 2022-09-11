using System.IdentityModel.Tokens.Jwt;
using IdentityModel.OidcClient;
using Microsoft.Extensions.Configuration;
using Volo.Abp.DependencyInjection;
using static System.String;
using DisplayMode = IdentityModel.OidcClient.Browser.DisplayMode;

namespace MauiBookStore.Services.OpenIddict
{
    public interface IOpenIddictService
    {
        Task<bool> AuthenticationSuccessful();
        Task LogoutAsync();
        Task<bool> IsUserLoggedInAsync();
    }
    
    [Volo.Abp.DependencyInjection.Dependency(ReplaceServices = true)]
    [ExposeServices(typeof(IOpenIddictService))]
    public class OpenIddictService : IOpenIddictService, ITransientDependency
    {
        private readonly IConfiguration _configuration;
        private readonly ISecureStorage _storageService;

        public OpenIddictService(IConfiguration configuration, ISecureStorage storageService)
        {
            _configuration = configuration;
            _storageService = storageService;
        }

        public async Task<bool> AuthenticationSuccessful()
        {
            var oidcClient = CreateOidcClient();
            var result = await oidcClient.LoginAsync(new LoginRequest());

            var isAuthenticated = !IsNullOrWhiteSpace(result.AccessToken) &&
                                  !IsNullOrWhiteSpace(result.IdentityToken) &&
                                  !IsNullOrWhiteSpace(result.RefreshToken);

            if (!isAuthenticated) return false;

            await _storageService.SetAsync("AccessToken", result.AccessToken);
            await _storageService.SetAsync("RefreshToken", result.RefreshToken);
            await _storageService.SetAsync("IdentityToken", result.IdentityToken);

            return true;
        }

        async Task IOpenIddictService.LogoutAsync()
        {
            var oidcClient = CreateOidcClient();
            try
            {
                var result = await oidcClient.LogoutAsync(new LogoutRequest
                {
                    IdTokenHint = await _storageService.GetAsync("IdentityToken"),
                    BrowserDisplayMode = DisplayMode.Hidden,
                });

                if (result.IsError) await Task.CompletedTask;
                else
                {
                    _storageService.Remove("AccessToken");
                    _storageService.Remove("RefreshToken");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<bool> IsUserLoggedInAsync() => await IsAccessTokenValidAsync();

        private async Task<bool> IsAccessTokenValidAsync()
        {
            var accessToken = await _storageService.GetAsync("AccessToken");
            if (accessToken.IsNullOrWhiteSpace()) return false;
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(accessToken);
            var isValid = token != null && token.ValidFrom < DateTime.UtcNow && token.ValidTo > DateTime.UtcNow;
            return isValid;
        }


        private OidcClient CreateOidcClient()
        {
            var oIddict = _configuration.GetSection(nameof(OpenIddictSettings)).Get<OpenIddictSettings>();
            var options = new OidcClientOptions
            {
                Authority = oIddict.AuthorityUrl,
                ClientId = oIddict.ClientId,
                Scope = oIddict.Scope,
                RedirectUri = oIddict.RedirectUri,
                ClientSecret = oIddict.ClientSecret,
                PostLogoutRedirectUri = oIddict.PostLogoutRedirectUri,
                Browser = new WebAuthenticatorBrowser()
            };

            return new OidcClient(options);
        }
    }
}