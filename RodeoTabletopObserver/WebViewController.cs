using Microsoft.Playwright;

namespace RodeoTabletopObserver;

public sealed class WebViewController : IDisposable
{
    private bool _disposedValue;

    private static string[] ExpectedHosts { get; } = new[]
    {
        ".owlbear.rodeo",
        ".owlbear.app",
    };
    private static string[] ItemsToHide { get; } = new[]
    {
        "#toolbar-container",
        "#actions",
        "div:has(> #extras-button)"
    };

    private IPlaywright? Playwright { get; set; }
    private IBrowser? Browser { get; set; }

    private IPage? Page => Browser?.Contexts.FirstOrDefault()?.Pages.FirstOrDefault();

    public Task Navigate(string url)
    {
        if (Page is { } page)
        {
            return page.GotoAsync(url);
        }
        return Task.CompletedTask;
    }

    public async Task ConnectAsync(string endpointUrl)
    {
        if (Playwright is not null)
        {
            throw new InvalidOperationException("Already connected");
        }

        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.ConnectOverCDPAsync(endpointUrl);
        if (Page is { } page)
        {
            await page.RouteAsync(_ => true, async route =>
            {
                Uri requestUri = new(route.Request.Url);
                if (ExpectedHosts.Any(x => requestUri.Host.EndsWith(x)) &&
                    requestUri.PathAndQuery.StartsWith("/assets/") &&
                    requestUri.PathAndQuery.EndsWith(".css"))
                {
                    IAPIResponse response = await route.FetchAsync();
                    string body = await response.TextAsync();
                    if (response.Ok)
                    {
                        foreach (var selector in ItemsToHide)
                        {
                            body += $$"""
                            {{selector}} {
                                visibility: collapse
                            }
                            """;
                        }
                    }
                    await route.FulfillAsync(new RouteFulfillOptions()
                    {
                        Body = body,
                        Status = response.Status,
                        Headers = response.Headers,
                    });
                }
                else
                {
                    await route.ContinueAsync();
                }
            });
        }

    }

    public async Task ResetViewAsync()
    {
        if (Page is { } page)
        {
            try
            {
                await page.PressAsync("body", "Control+Alt+R", new PagePressOptions
                {
                    Timeout = 5_000
                });
            }
            catch (TimeoutException)
            {
                //Ignoring timeouts
            }
        }
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                Playwright?.Dispose();
                Browser = null;
            }
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
