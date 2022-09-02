using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
// ReSharper disable InconsistentNaming

namespace MauiBookStore.Services.OpenIddict
{
    public class IdentityService : IIdentityService
    {
        private readonly IConfiguration _configuration;

        public IdentityService(IConfiguration configuration) => _configuration = configuration;

        public async Task<string> LoginAsync(string userName, string password)
        {
            var oiSettings = _configuration.GetSection(nameof(OpenIddictSettings)).Get<OpenIddictSettings>();
            var clientId = oiSettings.ClientId;
            var clientSecret = oiSettings.ClientSecret;
            var scope = oiSettings.Scope;
            var ngrokUrl = oiSettings.AuthorityUrl;

            var data = $"grant_type=password&username={userName}&password={password}&client_id={clientId}&client_secret={clientSecret}&scope={scope}";

            var content = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");

            var httpClient = new HttpClient(GetHttpClientHandler());
            var response = await httpClient.PostAsync($"{ngrokUrl}/connect/token", content);
            response.EnsureSuccessStatusCode();

            var stringResult = await response.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<IdentityDto>(stringResult, Options);

            return string.IsNullOrWhiteSpace(loginResult?.access_token) ? "UnAuthorized" : "Login Successful!";
        }

        private HttpClientHandler GetHttpClientHandler()
        {
            // EXCEPTION: Javax.Net.Ssl.SSLHandshakeException: 'java.security.cert.CertPathValidatorException:
            // Trust anchor for certification path not found.'
            // SOLUTION: 
            // ATTENTION: DO NOT USE IN PRODUCTION 

            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
            };

            return httpClientHandler;
        }

        private JsonSerializerOptions Options => new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

    }

    public class IdentityDto
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
        public string scope { get; set; }
        public string error { get; set; }
        public string error_description { get; set; }
    }
}

