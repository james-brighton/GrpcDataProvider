using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Browser;
using JamesBrighton.Data.GrpcTest.BrowserClient;

[assembly: SupportedOSPlatform("browser")]
internal partial class Program
{
    static async Task Main(string[] args) => await BuildAvaloniaApp().StartBrowserAppAsync("out");

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().WithInterFont();
}