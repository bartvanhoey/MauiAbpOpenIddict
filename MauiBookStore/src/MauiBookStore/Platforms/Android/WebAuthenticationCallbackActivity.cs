using Android.App;
using Android.Content.PM;
using Microsoft.Maui.Controls.PlatformConfiguration;

namespace MauiBookStore
{
    [Activity(Name ="MauiBookStore.WebAuthenticationCallbackActivity", NoHistory = true, LaunchMode = LaunchMode.SingleTop)]
    [IntentFilter(new[] { Android.Content.Intent.ActionView },
        Categories = new[] { Android.Content.Intent.CategoryDefault, Android.Content.Intent.CategoryBrowsable },
        DataScheme = App.CallbackUri)]
    public class WebAuthenticationCallbackActivity : WebAuthenticatorCallbackActivity
    {
    }
}