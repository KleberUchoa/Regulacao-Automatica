using Microsoft.Playwright;
using regulacao.core.browser;
using regulacao.core.services;

namespace regulacao.core.pages;

public class LoginPage
{
    private readonly BrowserSession? session;
    private readonly LogService? logService;
    public LoginPage() { }
    public LoginPage(BrowserSession session, LogService logService)
    {
        this.session = session;
        this.logService = logService;
    }
    public async Task<bool> Login(string site, string usuario, string senha)
    {
        var browserSession = session ?? throw new InvalidOperationException("A sessão do navegador não foi inicializada.");
        try
        {
            await browserSession.Page.GotoAsync(site);
            await browserSession.Page.FillAsync("#usuario_email", usuario);
            await browserSession.Page.FillAsync("#usuario_password", senha);
            await browserSession.Page.ClickAsync("button[type=submit]");
            await browserSession.Page.WaitForLoadStateAsync(LoadState.Load);

            // Se o form de login ainda está na página, é porque o / entrar
            // re-renderizou a mesma view (login falhou)
            bool aindaNaTelaDeLogin = await browserSession.Page.IsVisibleAsync("#new_usuario");

            if (aindaNaTelaDeLogin)
            {
                // tenta capturar a mensagem de erro (flash), se existir
                string? mensagemErro = null;
                var flash = browserSession.Page.Locator(".alert, .flash, [class*='alert-']").First;
                if (await flash.IsVisibleAsync())
                    mensagemErro = (await flash.InnerTextAsync()).Trim();

                logService?.AdicionarLinha("Falha ao realizar o login: " + mensagemErro);
            }

            var menu = browserSession.Page.Locator("//*[@id=\"header\"]/ul/li[3]/ul/li[3]/a");
            if (!await menu.IsVisibleAsync())
            {
                const string mensagemFalha = "Falha ao realizar o login: menu principal não ficou visível.";
                logService?.AdicionarLinha(mensagemFalha);
            }
            else
            {
                logService?.AdicionarLinha("O login foi realizado com sucesso!");
                return true;
            }

        }
        catch (TimeoutException)
        {
            logService?.AdicionarLinha("O servidor demorou para responder.");
        }
        catch (PlaywrightException ex)
        {
            logService?.AdicionarLinha("Erro durante o login: " + ex.Message);
        }
        catch (Exception ex)
        {
            logService?.AdicionarLinha("Erro inesperado: " + ex.Message);
        }
        return false;
    }
}
