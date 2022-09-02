using Android.App;
using Android.Runtime;

namespace MauiBookStore
{
#if DEBUG
    [Application(UsesCleartextTraffic = true)] // connect to local service
#else // on the host for debugging,
[Application]                               // access via http://10.0.2.2
#endif
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}