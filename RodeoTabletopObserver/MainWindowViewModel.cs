using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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
        
    }

    [RelayCommand(CanExecute = nameof(CanCloseDialog))]
    private void LoadRoom()
    {
        IsDialogOpen = false;
        if (Source is { } uri)
        {
            _webViewController?.Navigate(uri.AbsoluteUri);
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
