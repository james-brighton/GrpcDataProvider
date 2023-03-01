using Avalonia;
using Avalonia.Controls;

namespace JamesBrighton.Data.GrpcTest.SharedClient.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
		this.AttachDevTools();
#endif
    }
}