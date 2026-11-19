using regulacao.core.browser;
using regulacao.core.models;
using regulacao.core.pages;
using System.Drawing;

namespace regulacao.core.services;

public class RegulacaoService
{
    public static async Task<bool> ExecutarAsync(Padroes definicoes, List<Atendimento> atendimentos, Rectangle overlayBounds)
    {
        BrowserSession session = await BrowserFactory.CreateAsync(overlayBounds, true);
        LogService logService = new();

        LogoutPage logout = new (session, logService);
        LoginPage login = new (session, logService);
        bool loginRealizado = false;
        try
        {
            var formAtendimento = new FormularioAtendimentoPage(session, definicoes.Site, logService);
            var result = await login.Login(definicoes.Site, definicoes.Usuario, definicoes.Senha);
            if(!result)
            {
                return false;
            }
                
            var formulario = new FormularioAtendimentoPage(session, definicoes.Site, logService);
            await formulario.PreencherFormulario(definicoes, [.. atendimentos]);
        }
        catch (Exception ex)
        {
            logService.AdicionarLinha("Erro durante a execução: " + ex.Message);
            return false;
        }
        finally
        {
            if (loginRealizado)
                await logout.Logout();
            logService.SalvarLog();
            await session.Page.DisposeAsync();
            await session.Browser.CloseAsync();
            await session.Context.DisposeAsync();
            session.Playwright.Dispose();
        }
        return true;
    }
}
