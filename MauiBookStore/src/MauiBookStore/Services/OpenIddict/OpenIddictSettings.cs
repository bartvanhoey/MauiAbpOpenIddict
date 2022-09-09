namespace MauiBookStore.Services.OpenIddict
{
    public class OpenIddictSettings
    {
        public string AuthorityUrl { get; set; }
        public string ClientId { get; set; }
        public string RedirectUri { get; set; }
        public string Scope { get; set; }
        public string ClientSecret { get; set; }
        public string PostLogoutRedirectUri { get; set; }
    }
}