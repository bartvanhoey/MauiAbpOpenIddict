using Volo.Abp.DependencyInjection;

namespace MauiBookStore.Services
{
    public class HelloWorldService : ITransientDependency
    {
        public string SayHello()
        {
            return "Hello, World!";
        }
    }
}