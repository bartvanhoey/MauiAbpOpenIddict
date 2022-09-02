using Volo.Abp.DependencyInjection;

namespace MauiBookStore
{
    public class HelloWorldService : ITransientDependency
    {
        public string SayHello()
        {
            return "Hello, World!";
        }
    }
}