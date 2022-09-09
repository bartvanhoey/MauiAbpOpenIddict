using System.Reflection;
using MauiBookStore.Services.OpenIddict;
using MauiBookStore.ViewModels;
using MauiBookStore.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Volo.Abp;
using Volo.Abp.Autofac;

namespace MauiBookStore
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .ConfigureContainer(new AbpAutofacServiceProviderFactory(new Autofac.ContainerBuilder()));

            ConfigureConfiguration(builder);
        
            builder.Services.AddApplication<MauiBookStoreModule>(options =>
            {
                options.Services.ReplaceConfiguration(builder.Configuration);
            });

            builder.Services.AddSingleton<IOpenIddictService, OpenIddictService>();
            
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<LoginViewModel>();
            
            builder.Services.AddTransient<LogoutPage>();
            builder.Services.AddTransient<LogoutViewModel>();
            
            builder.Services.AddTransient<HomeViewModel>();
            
            var app = builder.Build();

            app.Services.GetRequiredService<IAbpApplicationWithExternalServiceProvider>().Initialize(app.Services);

            return app;
        }

        private static void ConfigureConfiguration(MauiAppBuilder builder)
        {
            var assembly = typeof(App).GetTypeInfo().Assembly;
            builder.Configuration.AddJsonFile(new EmbeddedFileProvider(assembly), "appsettings.json", optional: false,false);
        }
    }


}