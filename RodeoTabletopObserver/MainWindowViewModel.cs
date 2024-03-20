using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using RodeoTabletopObserver.Properties;

namespace RodeoTabletopObserver;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadRoomCommand))]
    private Uri? _source;

    [ObservableProperty]
    private bool _isDialogOpen = true;

    private WebViewController? _webViewController;

    public MainWindowViewModel()
    {
        if (Settings.Default.LastRoom is var lastRoom &&
            !string.IsNullOrWhiteSpace(lastRoom) &&
            Uri.TryCreate(lastRoom, UriKind.Absolute, out Uri? uri))
        {
            Source = uri;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCloseDialog))]
    private void LoadRoom()
    {
        IsDialogOpen = false;
        if (Source is { } uri)
        {
            _webViewController?.Navigate(uri.AbsoluteUri);
            Settings.Default.LastRoom = uri.AbsoluteUri;
            Settings.Default.Save();
        }
    }

    private bool CanCloseDialog() => Source is not null && _webViewController is not null;

    public void WebViewReady(WebViewController webViewController)
    {
        Interlocked.Exchange(ref _webViewController, webViewController)?.Dispose();
        LoadRoomCommand.NotifyCanExecuteChanged();
    }

    public Task ResetViewAsync()
    {
        return _webViewController?.ResetViewAsync() ?? Task.CompletedTask;
    }
}
