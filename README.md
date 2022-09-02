## ABP Framework (OpenIddict) consumed by .NET MAUI

## Introduction

From version 6.0.0 the ABP Framework will start to use **OpenIddict** instead of **IdentityServer**. In this article, I will show you how to consume an OpenIddict protected ABP Framework API from a .NET MAUI App.

The sample **BookStore ABP Framework** application in this article has been developed with **Blazor** as UI Framework and **SQL Server** as database provider.

### Source code

The source code of both projects is [available on GitHub](https://github.com/bartvanhoey/MauiAbpOpenIddict).

## Requirements

The following tools are needed to be able to run the solution and follow along.

* .NET 6.0 SDK
* VsCode, Visual Studio 2022 or another compatible IDE
* ABP CLI 6.0.0
* Ngrok

## ABP Framework application

### Create a new ABP Framework application

```bash
  abp new BookStore -u blazor -o BookStore
```

### Implement the Web Application Development tutorial (part1-5)

To follow along make sure you have a protected BookAppService in the BookStore application. For this article I followed the **Web Application Development tutorial** till **part 5: Authorization**.

### Add the section below in the appsettings.json file of the DbMigrator project

```bash
    "BookStore_Maui": {
        "ClientId": "BookStore_Maui",
        "ClientSecret": "1q2w3e*",
        "RootUrl": "bookstore://"
    }
```

### Add a MauiBookStore in the OpenIddictDataSeedContributor class of the Domain project

```bash
    // MauiBookStore
    var mauiClientId = configurationSection["BookStore_Maui:ClientId"];
    if (!mauiClientId.IsNullOrWhiteSpace())
    {
        var mauiRootUrl = configurationSection["BookStore_Maui:RootUrl"];

        await CreateApplicationAsync(
            name: mauiClientId,
            type: OpenIddictConstants.ClientTypes.Confidential,
            consentType: OpenIddictConstants.ConsentTypes.Implicit,
            scopes: commonScopes,
            grantTypes: new List<string>
            {
                OpenIddictConstants.GrantTypes.Password,
                OpenIddictConstants.GrantTypes.RefreshToken
            },
            secret: configurationSection["BookStore_Maui:ClientSecret"],
            redirectUri: $"{mauiRootUrl}",
            displayName: "MauiBookStore"
        );
    }
```

### Run DbMigrator project

To apply the settings above you need to run the DbMigrator project. After, check the **OpenIddictApplications** table of the database to see if the **BookStore_Maui** client has been added.

## .NET Core console application

### Create a new .NET Core console application

```bash
    dotnet new console -n MauiBookStore
```

### Install nuget packages (in terminal window or nuget package manager)

```bash
  dotnet add package IdentityModel.OidcClient --version 5.0.2
  dotnet add package Newtonsoft.Json --version 13.0.1
```

### Add a HttpService class in the root of the project

When you want to consume a protected API the user has to be **authenticated (username+password)** and **authorized(has the right permissions)**. So, when you call the BookAppService GetListAsync method, in the **header of the request** you need to send **the accesstoken** with.

To obtain the **accesstoken** you can make use of the **nuget package IdentityModel.OidcClient**. All the heavy lifting occurs in the **GetTokensFromBookStoreApi** method (See below). These method **sends a request** to the **disco.TokenEndpoint** of the BookStoreApi and **obtains a TokenResponse**. If the correct properties are sent and the API is running, you should obtain a **TokenResponse (AccessToken, IdentityToken, Scope, ...)**

Afterwards the obtained accesstoken is used in the **SetBearerToken()** of the httpClient.

When you make a request now to the protected BookStore API with the httpClient, the accesstoken is sent with. The BookStore API receives this request and checks the **validity of the accesstoken** and the **permissions**. If these conditions are met, the GetListAsync method of the BookAppService returns the list of books.

```csharp
using IdentityModel.Client;

namespace MauiBookStore
{
    public class HttpService
    {
        public async Task<Lazy<HttpClient>> GetHttpClientAsync(bool setBearerToken, string apiEndpoint)
        {
            var client = new Lazy<HttpClient>(() => new HttpClient());

            if (setBearerToken) client.Value.SetBearerToken(await GetAccessToken(apiEndpoint));
            
            client.Value.BaseAddress = new Uri(apiEndpoint); 
            return await Task.FromResult(client);
        }
        private static async Task<TokenResponse> GetTokensFromBookStoreApi(string apiEndpoint)
        {
            var discoveryCache = new DiscoveryCache(apiEndpoint);
            var disco = await discoveryCache.GetAsync();
            var httpClient = new Lazy<HttpClient>(() => new HttpClient());
            var response = await httpClient.Value.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint, // apiEndpoint/connect/token
                ClientId = "BookStore_Maui",
                ClientSecret = "1q2w3e*",
                UserName = "admin",
                Password = "1q2w3E*",
                Scope = "openid offline_access address email phone profile roles BookStore",
            });
            return response.IsError ?  new TokenResponse() : response;
        }
        private static async Task<string> GetAccessToken(string apiEndpoint) => (await GetTokensFromBookStoreApi(apiEndpoint)).AccessToken;
    }
}
```

### Main Method

Below you see the content of the **Program.cs** file. A new **HttpService** gets created and the **GetHttpClientAsync** method is called to get a httpClient.
Next, we make a request to the BookStore API to obtain the list of books.

Do not forget to change the apiEndpoint to the correct ABP Framework API endpoint (Swagger pager).

```csharp
using MauiBookStore;
using MauiBookStore.MauiBookStore;
using Newtonsoft.Json;
using static System.Console;
using static Newtonsoft.Json.JsonConvert;

// if setBearerToken = false, should throw JsonReaderException: 'json cannot be serialized.'
// if setBearerToken = true, API should be called an list of books should be returned
const bool setBearerToken = false;
const string apiEndpoint = "https://localhost:44317/";

var httpClient = await new HttpService().GetHttpClientAsync(setBearerToken, apiEndpoint);

var response = await httpClient.Value.GetAsync($"{apiEndpoint}api/app/book");
response.EnsureSuccessStatusCode();

var json = await response.Content.ReadAsStringAsync();
try
{
    var books = DeserializeObject<ListResultDto<BookDto>>(json);
    WriteLine("====================================");
    if (books?.Items != null)
        foreach (var book in books.Items)
            WriteLine(book.Name);
    WriteLine("====================================");

}
catch (JsonReaderException)
{
    WriteLine("Deserializing went wrong");
}


```

## Run API and .NET Core console application

Run the **BookStore.HttpApi.Host** of the ABP Framework application first. Start the .NET Core console application next. Below is the result when the accesstoken is successfully set.

![Books returned from API](../Images/books_returned_from_api.jpg)

If you set the variable **setBearerToken** to false, you will obtain a response from the API that **cannot be deserialized** and a **JsonReaderException** will be thrown.

Congratulations, you can now consume an OpenIddict protected ABP Framework API from a .NET Core console application!

Check out the [source code](https://github.com/bartvanhoey/AbpAddCustomClaimToAccessToken) of this article on GitHub.

Enjoy and have fun!
