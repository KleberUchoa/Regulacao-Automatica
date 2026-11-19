using Microsoft.Playwright;
using regulacao.core.browser;
using regulacao.core.services;

namespace regulacao.core.pages;

public class LogoutPage(BrowserSession session, LogService logService)
{
    private readonly BrowserSession session = session;
    private readonly LogService logService = logService;

    public async Task Logout()
    {
        try
        {
            var menu = session.Page.Locator("//*[@id=\"header\"]/ul/li[3]/ul/li[3]/a");
            if (!await menu.IsVisibleAsync())
            {
                logService.AdicionarLinha("Logout não realizado: menu principal não está visível.");
                return;
            }

            if (!await menu.IsEnabledAsync())
            {
                logService.AdicionarLinha("Logout não realizado: menu principal está desabilitado.");
                return;
            }

            await menu.ClickAsync();
            await Task.Delay(2000);
            await session.Page.GetByRole(AriaRole.Link, new() { Name = " Sair" }).ClickAsync(); //botão sair
            logService.AdicionarLinha("Logout realizado com sucesso.");
        }
        catch (Exception ex)
        {
            logService.AdicionarLinha("Falha ao realizar o logout: " + ex.Message);
        }

    }

}
