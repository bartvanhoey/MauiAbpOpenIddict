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