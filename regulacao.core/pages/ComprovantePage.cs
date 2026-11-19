using Microsoft.Playwright;
using regulacao.core.browser;
using regulacao.core.services;
using System.Text.RegularExpressions;

namespace regulacao.core.pages;



public partial class ComprovantePage
{
    private readonly BrowserSession? session;
    private readonly LogService? log;
    public ComprovantePage() { }
    public ComprovantePage(BrowserSession session, LogService? log)
    {
        this.session = session;
        this.log = log;
    }
    private static string CaminhoDownloads => Path.Combine(
     Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
     "Downloads");

    public async Task GerarComprovante(string site)
    {
        var browserSession = session ?? throw new InvalidOperationException("A sessão do navegador não foi inicializada.");
        var actionsDiv = browserSession.Page.Locator("div.block-header div.plain.actions div.btn-group").First;
        var btn = actionsDiv.Locator("a.btn.btn-primary.waves-effect").First;
        await Assertions.Expect(btn).ToBeVisibleAsync();

        var content = (await btn.TextContentAsync() ?? "").Trim();

        if (string.Equals(content, "COMPROVANTE", StringComparison.OrdinalIgnoreCase))
        {
            var link = site + "/" + await btn.GetAttributeAsync("href");
            var numeroProtocolo = GerarRex().Match(link).Value;
            var response = await browserSession.Context.APIRequest.GetAsync(link);

            if (!response.Ok)
                throw new Exception("Erro ao obter o comprovante.");

            var bytes = await response.BodyAsync();

            var caminho = Path.Combine(
                CaminhoDownloads,
                $"Comprovante-{numeroProtocolo}.pdf");

            await File.WriteAllBytesAsync(caminho, bytes);

        }
        else if (string.Equals(content, "AGENDAR", StringComparison.OrdinalIgnoreCase))
        {
            // dropdown-toggle é IRMÃO do btn, não filho — busca a partir do btn-group pai
            var btnDropDown = actionsDiv.Locator(".btn.btn-primary.dropdown-toggle.waves-effect");
            if (await btnDropDown.IsVisibleAsync())
            {
                await Assertions.Expect(btnDropDown).ToBeVisibleAsync();
                await btnDropDown.ClickAsync();
                var comprovanteLink = browserSession.Page.Locator("ul.dropdown-menu.pull-right > li a:has-text('Comprovante de Agendamento')");
                var link = site + await comprovanteLink.GetAttributeAsync("href");
                var numeroProtocolo = GerarRex().Match(link).Value;
                var response = await browserSession.Context.APIRequest.GetAsync(link);

                if (!response.Ok)
                    throw new Exception("Erro ao obter o comprovante.");

                var bytes = await response.BodyAsync();

                var caminho = Path.Combine(
                    CaminhoDownloads,
                    $"Comprovante-{numeroProtocolo}.pdf");

                await File.WriteAllBytesAsync(caminho, bytes);
                log?.AdicionarLinha($"Comprovante de agendamento gerado com sucesso: {caminho}");
            }
            else
            {
                log?.AdicionarLinha("Botão de dropdown para gerar comprovante não encontrado.");
            }
        }
        else
        {
            log?.AdicionarLinha("Botão de comprovante ou agendamento não encontrado.");
        }
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex GerarRex();
}
