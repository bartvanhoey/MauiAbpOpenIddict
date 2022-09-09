using BookStore;
using Volo.Abp.Autofac;
using Volo.Abp.Http.Client.IdentityModel;
using Volo.Abp.Modularity;

namespace MauiBookStore
{
    [DependsOn(
        typeof(AbpAutofacModule), 
        typeof(BookStoreHttpApiClientModule),
        typeof(AbpHttpClientIdentityModelModule))]
    public class MauiBookStoreModule : AbpModule
    {
    }
}
