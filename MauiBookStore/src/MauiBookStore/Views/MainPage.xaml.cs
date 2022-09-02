using MauiBookStore.ViewModels;
using Volo.Abp.DependencyInjection;

namespace MauiBookStore.Views
{
	public partial class MainPage : ISingletonDependency
	{
		public MainPage(MainViewModel mainViewModel)
		{
			BindingContext = mainViewModel;		
			InitializeComponent();
		}
	}
}
