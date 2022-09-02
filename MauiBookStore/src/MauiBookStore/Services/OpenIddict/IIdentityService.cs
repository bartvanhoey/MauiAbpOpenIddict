namespace MauiBookStore.Services.OpenIddict
{
    public interface IIdentityService
    {
        Task<string> LoginAsync(string userName, string password);
    }
}