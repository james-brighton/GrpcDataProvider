using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Web;
using JamesBrighton.Data.GrpcTest.WebClient;

[assembly: SupportedOSPlatform("browser")]
internal partial class Program
{
    static void Main(string[] args) => BuildAvaloniaApp().SetupBrowserApp("out");

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>();
}