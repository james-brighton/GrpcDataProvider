using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using JamesBrighton.Data.GrpcTest.SharedClient.Views;

namespace JamesBrighton.Data.GrpcTest.BrowserClient;

public partial class App : Application
{
	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);
	}

	public override void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
		{
			singleViewPlatform.MainView = new MainView();
		}

		base.OnFrameworkInitializationCompleted();
	}
}