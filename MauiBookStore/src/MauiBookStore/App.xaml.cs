namespace MauiBookStore
{
	public partial class App : Application
	{
		public const string CallbackUri = "bookstore://";

		public App()
		{
			InitializeComponent();

			MainPage = new AppShell();
		}
	}
}
