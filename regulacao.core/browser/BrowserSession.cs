using Microsoft.Playwright;

namespace regulacao.core.browser;

public class BrowserSession
{
    public IPlaywright Playwright { get; }

    public IBrowser Browser { get; }

    public IBrowserContext Context { get; }

    public IPage Page { get; }

    public BrowserSession(
        IPlaywright playwright,
        IBrowser browser,
        IBrowserContext context,
        IPage page)
    {
        Playwright = playwright;
        Browser = browser;
        Context = context;
        Page = page;
    }
}
