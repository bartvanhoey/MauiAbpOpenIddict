using IdentityModel.OidcClient.Browser;
using IBrowser = IdentityModel.OidcClient.Browser.IBrowser;

namespace MauiBookStore.Services.OpenIddict
{
    internal class WebAuthenticatorBrowser : IBrowser
    {
        private readonly string _callbackUrl;
        public WebAuthenticatorBrowser(string callbackUrl = null) => _callbackUrl = callbackUrl ?? "";

        public async Task<BrowserResult> InvokeAsync(BrowserOptions options,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var callbackUrl = string.IsNullOrEmpty(_callbackUrl) ? options.EndUrl : _callbackUrl;
                
                var authResult =
                    await WebAuthenticator.AuthenticateAsync(new Uri(options.StartUrl), new Uri(callbackUrl));
                var authorizeResponse = ToRawIdentityUrl(options.EndUrl, authResult);
                return new BrowserResult
                {
                    Response = authorizeResponse
                };
            }
            catch (TaskCanceledException ex)
            {
                return new BrowserResult
                {
                    ResultType = BrowserResultType.UnknownError,
                    Error = ex.ToString()
                };
            }
            catch (Exception ex)
            {
                return new BrowserResult
                {
                    ResultType = BrowserResultType.UnknownError,
                    Error = ex.ToString()
                };
            }
        }

        private static string ToRawIdentityUrl(string redirectUrl, WebAuthenticatorResult result)
        {
            var parameters = result.Properties.Select(pair => $"{pair.Key}={pair.Value}");
            var values = string.Join("&", parameters);
            return $"{redirectUrl}#{values}";
        }
    }
}