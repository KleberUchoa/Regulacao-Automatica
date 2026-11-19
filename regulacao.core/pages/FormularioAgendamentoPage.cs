using Microsoft.Playwright;
using regulacao.core.browser;
using regulacao.core.models;
using regulacao.core.services;
using System.Diagnostics;

namespace regulacao.core.pages;

public class FormularioAgendamentoPage
{
    private readonly BrowserSession? session;
    private readonly DateTime? data;
    private readonly LogService? log;
    private readonly string? nomePaciente;
    public FormularioAgendamentoPage() { }
    public FormularioAgendamentoPage(BrowserSession session, DateTime? dataAgendamento, String? nomePaciente, LogService? log)
    {
        this.session = session;
        data = dataAgendamento;
        this.log = log;
        this.nomePaciente = nomePaciente;
    }
    public async Task PreencherFormulario(Padroes padroes)
    {
        var browserSession = session ?? throw new InvalidOperationException("A sessão do navegador não foi inicializada.");
        //Página Agendar atendimento
        var fields = browserSession.Page.Locator("form.simple_form.edit_atendimento > .fields");

        for (int i = 0; i < await fields.CountAsync(); i++)
        {
            var field = fields.Nth(i);
            var estabelecimento = field.Locator(".select2-selection");
            await Assertions.Expect(estabelecimento).ToBeVisibleAsync();
            await estabelecimento.ClickAsync();

            var input = browserSession.Page.Locator(".select2-search__field");
            await Assertions.Expect(input).ToBeVisibleAsync();
            await input.FillAsync(padroes.EstabelecimentoRequisitado ?? "");
            // Aguarda os resultados
            await browserSession.Page.WaitForTimeoutAsync(1000);
            var opcao = browserSession.Page.Locator(".select2-results__option");
            var text = await opcao.TextContentAsync();
            if (text == "Nenhum resultado encontrado")
            {
                // Fecha o Select2
                await browserSession.Page.Keyboard.PressAsync("Escape");
                Debug.WriteLine($"Procedimento {i}: estabelecimento não disponível.");
                continue;
            }
            await opcao.First.ClickAsync();
            string dataDesejada = data?.ToString("dd/MM/yyyy") ?? "";
            bool encontrou = false;

            var linhas = field.Locator("table.table-condensed tbody tr");
            await Assertions.Expect(linhas.Nth(0)).ToBeVisibleAsync();
            int count = await linhas.CountAsync();
            for (int j = 0; j < count; j++)
            {
                var linha = linhas.Nth(j);
                var radioInput = linha.Locator("td").Nth(0).Locator("input[type='radio']");
                var dataTabela = (await linha.Locator("td").Nth(1).InnerTextAsync()).Trim().Split(' ')[0];

                Debug.WriteLine($"Desejada: {dataDesejada}");
                Debug.WriteLine($"Tabela:   {data}");

                if (dataDesejada == dataTabela)
                {
                    await Assertions.Expect(radioInput).ToBeVisibleAsync();
                    await radioInput.CheckAsync();
                    encontrou = true;
                    break;
                }
            }
            if (!encontrou)
            {
                //escolher a data manualmente
                await browserSession.Page.PauseAsync();
            }
        }
        await browserSession.Page.ClickAsync("//button[@type=\"submit\"]", new PageClickOptions
        {
            Timeout = 60000
        });


        var divErro = browserSession.Page.Locator(".alert.alert-danger");
        if(await divErro.IsVisibleAsync())
        {
            var divAlerta = browserSession.Page.Locator("#flash_alert");
            if(await divAlerta.IsVisibleAsync())
            {
                var textoAlerta = await divAlerta.TextContentAsync();
                log?.AdicionarLinha(textoAlerta ?? "");
            }
            return;
        }
        else
        {
            log?.AdicionarLinha($"Paciente {nomePaciente} agendado com sucesso." ?? "");
        }

        var comprovantePage = new ComprovantePage(browserSession, log);
        await comprovantePage.GerarComprovante(padroes.Site);
    }
}
