using Microsoft.Playwright;
using System.Drawing;

namespace regulacao.core.browser;

public class BrowserFactory
{
    public static async Task<BrowserSession> CreateAsync(Rectangle? overlayBounds = null, bool maximizado = false)
    {
        var playwright = await Playwright.CreateAsync();
        var args = new List<string>();
        if (overlayBounds is Rectangle b)
        {
            args.Add($"--window-position={b.X},{b.Y}");
            args.Add($"--window-size={b.Width},{b.Height}");
        }//argumento para por o browser sobre a janela principal do programa
        if (maximizado)
        {
            args.Add("--start-maximized");
        }
        var browser = await playwright.Chromium.LaunchAsync(
            new()
            {
                Headless = false,
                Args = args,
            });
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            AcceptDownloads = true,
            ViewportSize = ViewportSize.NoViewport
        });
        var page = await context.NewPageAsync();
        return new BrowserSession(playwright, browser, context, page);
    }
}