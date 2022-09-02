using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace MauiBookStore
{
    [DependsOn(typeof(AbpAutofacModule))]
    public class MauiBookStoreModule : AbpModule
    {
    }
}
