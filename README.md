## Consume an ABP API (OpenIddict) from a .NET MAUI app

## Introduction

In a previous article, I demonstrated how you could consume an ABP Framework API with a .NET MAUI app by providing a username and password in a .NET MAUI page.
In this article, I will show you a more **secure way of authentication** where we make use of a web browser in the app that **redirects us to the ABP Framework login page**.

From version 6.0.0 the ABP Framework comes with a [MAUI Application Startup Template](https://docs.abp.io/en/abp/6.0/Startup-Templates/MAUI) to create a minimalist **.NET MAUI** application project and ABP also start to use **OpenIddict** instead of **IdentityServer**.

I will only mention the main moving parts of the program. Keep in mind that only the most important code snippets are included in this article, but you will find the rest of the code needed in the GitHub repository.

### Source code

The source of the article is [available on GitHub](https://github.com/bartvanhoey/MauiAbpOpenIddict), but keep in mind that the source code is not production ready.

## Requirements

The following tools are needed to be able to run the solution and follow along.

- .NET 6.0 SDK
- vscode, Visual Studio 2022 or another compatible IDE
- ABP CLI 6.0.0
- Ngrok

## ABP Framework application

### Create a new ABP Framework application

```bash
  abp new BookStore -u blazor -o BookStore --no-ui --preview
```

### Appsettings.json file of the DbMigrator project

Add the section below in the appsettings.json file of the DbMigrator project

```bash
    "BookStore_Maui": {
        "ClientId": "BookStore_Maui",
        "ClientSecret": "1q2w3e*",
        "RootUrl": "bookstore://"
    }
```

### Update the OpenIddictDataSeedContributor class of the Domain project

Add a MauiBookStore client section in the OpenIddictDataSeedContributor class of the Domain project

```bash
    // MauiBookStore client
    var mauiScopes = new List<string>
    {
        "offline_access",
        OpenIddictConstants.Permissions.Scopes.Address,
        OpenIddictConstants.Permissions.Scopes.Email,
        OpenIddictConstants.Permissions.Scopes.Phone,
        OpenIddictConstants.Permissions.Scopes.Profile,
        OpenIddictConstants.Permissions.Scopes.Roles,
        "BookStore"
    };

    var mauiClientId = configurationSection["BookStore_Maui:ClientId"];
    if (!mauiClientId.IsNullOrWhiteSpace())
    {
        var mauiRootUrl = configurationSection["BookStore_Maui:RootUrl"];
        await CreateApplicationAsync(
            name: mauiClientId,
            type: OpenIddictConstants.ClientTypes.Confidential,
            consentType: OpenIddictConstants.ConsentTypes.Implicit,
            scopes: mauiScopes,
            grantTypes: new List<string>
            {
                OpenIddictConstants.GrantTypes.AuthorizationCode,
                OpenIddictConstants.GrantTypes.RefreshToken
            },
            secret: configurationSection["BookStore_Maui:ClientSecret"],
            redirectUri: $"{mauiRootUrl}",
            postLogoutRedirectUri: $"{mauiRootUrl}",
            displayName: "MauiBookStore"
        );
    }
```

### Apply Migrations and Run the Application

- To apply the settings above you need to run the DbMigrator project. After, check the **OpenIddictApplications** table of the database to see if the **BookStore_Maui** client has been added.
- Run the `BookStore.HttpApi.Host` application to start the API.

## Ngrok to the rescue

When you run the **ABP Framework API** on your local computer, the API is reachable on [https://localhost:\<your-port-number\>/api/\<path\>](https://localhost:<your-port-number>/api/<path>). Although you can test out the **API endpoints** on your local machine, it will throw an exception in a .NET MAUI app.

```bash
    System.Net.WebException: Failed to connect to localhost/127.0.0.1:44330 ---> Java.Net.ConnectException: Failed to connect to localhost/127.0.0.1:44330
```

.NET MAUI considers localhost as its own localhost address (mobile device or emulator). To overcome this problem you can **make use of ngrok** to **mirror your localhost address to a publicly available url**.

Go to the [ngrok page](https://ngrok.com/), create an account, and download and install Ngrok.

Open a command prompt in the root of the ABP Framework project and enter the command below to start ngrok

```bash
    // change the <replace-me-with-the-abp-api-port> with the port where the Swagger page is running on
    ngrok.exe http -region eu https://localhost:<replace-with-the-abp-api-port-number>/
```

The **ABP Framework API** is now publicly available on [https://f7db-2a02-810d-98c0-576c-647e-cd22-5b-e9a3.eu.ngrok.io](https://f7db-2a02-810d-98c0-576c-647e-cd22-5b-e9a3.eu.ngrok.io)

![ngrok in action](Images/ngrok.jpg)

### Copy the ngrok url

Copy the **lower forwarding url** as you will need it for use in the .NET MAUI app.

## .NET MAUI app

### Create a new .NET MAUI app

```bash
    abp new MauiBookStore -t maui -o MauiBookStore --preview
```

### Let's Install some NuGet packages (in terminal window or NuGet package manager)

```bash
    dotnet add package System.IdentityModel.Tokens.Jwt --version 6.23.0
    dotnet add package CommunityToolkit.Diagnostics --version 8.0.0
    dotnet add package CommunityToolkit.Mvvm --version 8.0.0
    dotnet add package IdentityModel --version 6.0.0
    dotnet add package IdentityModel.OidcClient --version 5.0.2
    dotnet add package Microsoft.Extensions.Configuration --version 6.0.1
    dotnet add package Microsoft.Extensions.Configuration.Binder --version 6.0.0
    dotnet add package Microsoft.Extensions.Configuration.Json --version 6.0.0
```

### Add an OpenIddictSettings section to the appsettings.json file of the MAUI app

```bash
"OpenIddictSettings": {
    "AuthorityUrl": "http://<replace-me-with-the-correct-url>.eu.ngrok.io",
    "ClientId" : "BookStore_Maui",
    "RedirectUri" : "bookstore://",
    "Scope" : "openid offline_access address email profile roles BookStore",
    "ClientSecret" : "1q2w3e*"
}
```

### Add a StorageService class to the Services/Storage folder

```csharp
using Volo.Abp.DependencyInjection;

namespace MauiBookStore.Services.Storage
{
    [Volo.Abp.DependencyInjection.Dependency(ReplaceServices = true)]
    [ExposeServices(typeof(ISecureStorage))]
    public class StorageService : ISecureStorage, ITransientDependency
    {
        public Task<string> GetAsync(string key)
        {
#if DEBUG
                var fromResult = Task.FromResult(Preferences.Get(key, string.Empty));
                return fromResult;
#else
        return SecureStorage.GetAsync(key);
#endif
        }

        public bool Remove(string key)
        {
#if DEBUG
            Preferences.Remove(key, string.Empty);
            return true;
#else
        return SecureStorage.Remove(key);
#endif
        }

        public void RemoveAll()
        {
#if DEBUG
            Preferences.Clear();
#else
        return SecureStorage.RemoveAll();
#endif
        }

        public Task SetAsync(string key, string value)
        {
#if DEBUG
            Preferences.Set(key, value);
            return Task.CompletedTask;
#else
        return SecureStorage.SetAsync(key);
#endif
        }
    }
}
```

### Add an OpenIddictSettings class to the Services/OpenIddict folder

```csharp
public class OpenIddictSettings
{
    public string AuthorityUrl { get; set; }
    public string ClientId { get; set; }
    public string RedirectUri { get; set; }
    public string Scope { get; set; }
    public string ClientSecret { get; set; }
    public string PostLogoutRedirectUri { get; set; }
}
```

### Add a WebAuthenticatorBrowser class to the Services/OpenIddict folder

```csharp
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
```

### Add an IOpenIddictService interface/class to the Services/OpenIddict folder

```csharp
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
```

### Copy/paste the Views (+ code-behind pages) and ViewModels from the source code

- LoginPage.xaml/LoginPage.xaml.cs/LoginViewModel.cs
- LogoutPage.xaml/LogoutPage.xaml.cs/LogoutViewModel.cs
- HomePage.xaml/HomePage.xaml.cs

### Replace the content of the AppShell page

```bash
<?xml version="1.0" encoding="UTF-8" ?>
<Shell
    x:Class="MauiBookStore.AppShell"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:views="clr-namespace:MauiBookStore.Views">

    <ShellContent ContentTemplate="{DataTemplate views:LoginPage}" FlyoutItemIsVisible="False" Route="LoginPage" />
    <ShellContent Title="Home" ContentTemplate="{DataTemplate views:HomePage}" Route="HomePage" />
    <ShellContent Title="Logout" ContentTemplate="{DataTemplate views:LogoutPage}" FlyoutItemIsVisible="True" Route="LogoutPage" />

</Shell>
```

### Register the Pages and ViewModels in MauiProgram.cs

```csharp
    \\ ... other code here

    builder.Services.AddTransient<LoginPage>();
    builder.Services.AddTransient<LoginViewModel>();

    builder.Services.AddTransient<LogoutPage>();
    builder.Services.AddTransient<LogoutViewModel>();

    var app = builder.Build();
```

### WebAuthenticationCallbackActivity in the root of the Android project

Add a WebAuthenticationCallbackActivity class in the root of the Android project.

```csharp
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
```

### Create a CallBackUri const in App.xaml.cs

```csharp
    public const string CallbackUri = "bookstore://";
```

### Update the AndroidManifest.xml file

I you try running your app now, you will probably get the error below:

```bash
AndroidManifest.xml(19, 5): [AMM0000]
android:exported needs to be explicitly specified for element <activity#MauiBookStore.WebAuthenticationCallbackActivity>.

Apps targeting Android 12 and higher are required to specify an explicit value for `android:exported` when the corresponding component has an intent filter defined.

See https://developer.android.com/guide/topics/manifest/activity-element#exported for details.

```

To overcome this problem, update your **AndroidManifest.xml** file

```bash
<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android">
    <application android:allowBackup="true" android:icon="@mipmap/appicon" android:roundIcon="@mipmap/appicon_round"
                 android:supportsRtl="true">

        <activity android:exported="true" android:launchMode="singleTop" android:noHistory="true"
                  android:name="MauiBookStore.WebAuthenticationCallbackActivity">
            <intent-filter>
                <action android:name="android.intent.action.VIEW"/>
                <category android:name="android.intent.category.DEFAULT"/>
                <category android:name="android.intent.category.BROWSABLE"/>
                <data android:scheme="bookstore"/>
            </intent-filter>
        </activity>

    </application>
    <uses-permission android:name="android.permission.ACCESS_NETWORK_STATE"/>
    <uses-permission android:name="android.permission.INTERNET"/>

    <queries>
        <intent>
            <action android:name="android.support.customtabs.action.CustomTabsService" />
        </intent>
    </queries>
</manifest>
```

## Test the result

Run the **HttpApi.Host** project and make sure **Ngrok** is running too.

Start the **.NET Maui app** and click the **Login** button to display the ABP Framework login page.

Enter the standard credentials (user name: **admin** - password: **1q2w3E\***) and click Login.
You will be redirected to the HomePage of the app.




Et voilà! As you can see, you received an access token from the **ABP Framework API**. Now you can start consuming the API!

Get the [source code](https://github.com/bartvanhoey/MauiAbpOpenIddict) on GitHub.

Enjoy and have fun!

### Extra Goody

If you are a **Mobile Developer** and you make use of **Ngrok to connect with your API**, you always need to **copy/paste the Ngrok** url and paste it in your appsettings.file of your mobile project.

I decided to write a little **Batch script** (Windows only) to automate this process.

#### Create Ngrok.bat file

Create a file Ngrok.bat in the root of your ABP Framework project and paste into the code below. You will probably also need to install **jq** (Command-line JSON processor).

```bash
@echo off
set sourceFile="C:\<your-path-to-the-appsettings-file-here>\appsettings.json"
set portNumber=<api-port-number-here>

setlocal disabledelayedexpansion

start ngrok.exe http -region eu https://localhost:%portNumber%/

timeout 5 > NUL

if not exist "C:\TEMP_NGROK\" mkdir "C:\TEMP_NGROK\"

for /F %%I in ('curl -s http://127.0.0.1:4040/api/tunnels ^| jq -r .tunnels[0].public_url') do set ngrokTunnel=%%I
echo %ngrokTunnel%
echo %ngrokTunnel%/.well-known/openid-configuration

for /f "tokens=1* delims=]" %%a in ('find /n /v "" %sourceFile%') do (
  echo %%b|findstr /rc:"\ *\"AuthorityUrl\".*\:\ \".*\"" >nul && (
    for /f "delims=:" %%c in ("%%b") do echo %%c: "%ngrokTunnel%",
    ) || echo/%%b
)>>C:\TEMP_NGROK\temp.json 2>nul

for %%I in (C:\TEMP_NGROK\temp.json) do for /f "delims=, tokens=* skip=1" %%x in (%%I) do echo %%x >> "%%I.new"

move /Y "C:\TEMP_NGROK\temp.json.new" %sourceFile% > nul

del C:\TEMP_NGROK\temp.json
rmdir /s /q "C:\TEMP_NGROK\"

```

#### How to use

Start the ABP Framework API and open a command prompt in **the root of your ABP project** and enter the command below. Your Ngrok url will automatically replace the **AuthorityUrl** of your appsettings file of your mobile project.

```bash
    ngrok
```