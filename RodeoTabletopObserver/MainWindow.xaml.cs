using System.Windows.Controls;
using System.Windows.Input;

using Microsoft.Playwright;
using Microsoft.Web.WebView2.Core;

namespace RodeoTabletopObserver;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        
        WebView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
        WebView.NavigationCompleted += WebView_NavigationCompleted;
    }

    private void WebView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        WebView.NavigationCompleted -= WebView_NavigationCompleted;
        WebView.Height = double.NaN;
    }


    private async void MainWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        await WebView.EnsureCoreWebView2Async(await CoreWebView2Environment.CreateAsync(null, null, new CoreWebView2EnvironmentOptions()
        {
            AdditionalBrowserArguments = "--remote-debugging-port=9222",
        })).ConfigureAwait(false);
    }

    private async void WebView_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
    {
        if (e.IsSuccess)
        {
            WebViewController controller = new();
            await controller.ConnectAsync("http://localhost:9222");
            ((MainWindowViewModel)DataContext).WebViewReady(controller);

        }
    }

    private async void Window_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
    {
        await ((MainWindowViewModel)DataContext).ResetViewAsync();
    }
}
