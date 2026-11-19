using listar_exames.helpers;
using regulacao.core.models;
using regulacao.core.services;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using UI;

namespace listar_exames;

public partial class FormPrincipal : Form
{
    private void Atendimentos_ListChanged(object? sender, ListChangedEventArgs e)
    {
        listaFoiModificada = true;
        listaFoiSalva = false;

        if (e.ListChangedType == ListChangedType.ItemAdded)
            BeginInvoke(() => AtualizarProcedimentosGrid(e.NewIndex));
        else
            BeginInvoke(() => AtualizarProcedimentosGrid());
    }
    private void BtnAdicionarAtendimento_Click(object? sender, EventArgs e)
    {
        AbrirFormularioAdicionarAtendimento();
    }
    private async void BtnEnviarParaRegulacao_Click(object? sender, EventArgs e)
    {
        if (atendimentos.Count == 0)
            return;
        var definicoes = padroesCarregados;
        if (definicoes is null)
        {
            Alerta.Show("Erro", "As definições da regulação ainda não foram carregadas.", AlertaType.Error);
            return;
        }

        Enabled = false;
        SalvarListaDeAtendimento();
        try
        {
            var enviadoComSucesso = await RegulacaoService.ExecutarAsync(definicoes, [.. atendimentos], Bounds);
            if(enviadoComSucesso)
            {
                LimparListaDeAtendimento();
                Alerta.Show("Sucesso", "Lista enviada com sucesso para o sistema de regulação.", AlertaType.Success);
                listaFoiModificada = false;
                listaFoiSalva = false;
            }
        }
        catch (Exception ex)
        {
            Alerta.Show("Erro", "Falha ao enviar ao sistema de regulação: " + ex.Message, AlertaType.Error);
        }
        finally
        {
            Enabled = true;
        }
        
    }
    private void LimparListaDeAtendimento()
    {
        atendimentos.Clear();
        atendimentosData = new();
        arquivoAtual = null;
    }
    private void BtnLimparListaDeAtendimento_Click(object? sender, EventArgs e)
    {
        if (atendimentos.Count == 0)
            return;
        var resposta = Components.CriarDialogBoxConfimacao(
            "Limpar",
            "Deseja realmente limpar a lista de atendimentos? \nVocê perderá todos os dados não salvos.");
        if (resposta == DialogResult.OK)
        {
            LimparListaDeAtendimento();
            listaFoiModificada = false;
            listaFoiSalva = false;
        }
    }
    
    private void BtnSalvarListaDeAtendimento_Click(object? sender, EventArgs e)
    {
        //clique explícito do usuário: salva sempre, mesmo que listaFoiModificada
        //esteja false (ex: logo após recuperar uma lista salva)
        SalvarListaDeAtendimento(forcar: true);
        LimparListaDeAtendimento();
    }
    private void SalvarListaDeAtendimento(bool forcar = false)
    {
        if (atendimentos.Count == 0)
            return;

        if (!forcar && !listaFoiModificada)
            return;

        atendimentosData.Atendimentos = [.. atendimentos];

        var json = JsonSerializer.Serialize(
            atendimentosData, jsonOptions);


        string caminho;

        if (!string.IsNullOrEmpty(arquivoAtual))
        {
            //sobrescreve
            caminho = arquivoAtual;
        }
        else
        {
            //novo arquivo
            caminho = Path.Combine(
                pastaAtendimentos,
                $"atendimento-{atendimentosData.Id}.json");

            arquivoAtual = caminho;
        }

        File.WriteAllText(caminho, json);

        listaFoiModificada = false;
        listaFoiSalva = true;

        Alerta.Show(
            "Salvo",
            "Lista salva com sucesso.",
            AlertaType.Success);
    }
    private List<Procedimento> PegarProcedimentosAdicionados()
    {
        //retorna apenas os itens selecionados de painel de adicionar procedimentos
        if (flowLayoutPanelProcedimentos is null)
            return [];

        return [..flowLayoutPanelProcedimentos.Controls
            .OfType<Panel>()
            .Select(p => p.Controls.OfType<ComboBox>().FirstOrDefault())
            .OfType<ComboBox>()
            .Select(c => c.SelectedItem)
            .OfType<Procedimento>()
            .Where(p => !string.IsNullOrWhiteSpace(p.Codigo))];
    }
    private bool ValidarCampos(out string texto)
    {
        var sb = new StringBuilder();
        if (chkNome is null || chkCpf is null || chkCns is null || txtNomePaciente is null ||
            txtCpfPaciente is null || txtCnsPaciente is null || dtpDataExame is null ||
            txtHistorico is null || cmbMedicos is null)
        {
            texto = "O formulário de atendimento não está pronto.";
            return false;
        }

        if (chkNome.Checked && !Validacao.ValidarCampoObrigatorio(txtNomePaciente.Text))
            sb.AppendLine("• O nome precisa ser informado, visto que o campo \"Nome\" está habilitado");
        if (chkCpf.Checked && !Validacao.ValidarCpf(txtCpfPaciente.Text))
            sb.AppendLine("• CPF inválido. Para não informar basta desabilitar o campo \"CPF\"");
        if (chkCns.Checked && !Validacao.ValidarCNS(txtCnsPaciente.Text))
            sb.AppendLine("• CNS inválido. Para não informar basta desabilitar o campo \"CNS\"");
        if (!Validacao.ValidarData(dtpDataExame.Value.Date))
            sb.AppendLine("• A data não pode ser a atual ou inferior a ela");
        if (!Validacao.ValidarCampoObrigatorio(txtHistorico.Text))
            sb.AppendLine("• É preciso informar o historico clínico");
        if (!Validacao.ValidarPossuiDocumento(txtCpfPaciente.Text, txtCnsPaciente.Text))
            sb.AppendLine("• É preciso informar Cpf ou Cns para identificar o paciente");
        if (!Validacao.ValidarCampoObrigatorio(cmbMedicos.SelectedValue?.ToString() ?? ""))
            sb.AppendLine("• É preciso informar o Médico Solicitante");

        if (!Validacao.ValidarProcedimentosSelecionados(PegarProcedimentosAdicionados(), out var msn))
            sb.AppendLine($"• Erro ao adicionar procedimentos: {msn}");
        texto = sb.ToString();
        return sb.Length == 0;
    }
    private async void BtnSalvarAtendimento_Click(object? sender, EventArgs e)
    {
        string mensagem = string.Empty;
        if (!ValidarCampos(out mensagem))
        {
            mensagemUI?.MostrarErro(mensagem, 8);
            if (mensagemUI is not null)
                await mensagemUI.LimparMensagem(20000);
            return;
        }
        Atendimento atendimento = CriarAtendimento();
        if (editMode)
        {
            if (!int.TryParse(txtId?.Text, out int id))
            {
                mensagemUI?.MostrarErro("Não foi possível identificar o atendimento a editar.");
                return;
            }
            //remove a antiga versão e adiciona uma nova
            var antigo = atendimentos.FirstOrDefault(p => p.Id == id);
            if (antigo != null)
            {
                int indice = atendimentos.IndexOf(antigo);
                atendimentos[indice] = atendimento;
            }
            editMode = false;
        }
        else
            atendimentos.Add(atendimento); //Adiciona o novo atendimento
        DestruirFormularioAdicionarAtendimento();
        if (btnAdicionarAtendimento is not null)
            btnAdicionarAtendimento.Enabled = true;
    }
    private void BtnRecuperarAtendimento_Click(object? sender, EventArgs e)
    {
        RecuperarAtendimentos();
    }
    private void RecuperarAtendimentos()
    {
        Dictionary<string, string> lista = ListaArquivos();

        var dlg = new DialogUI(
            "Recuperar Lista de Atendimentos",
            400,
            600,
            AppResources.Check,
            AppResources.Minus,
            AppResources.Cross,
            Color.LightGoldenrodYellow);

        var lvw = Components.CriarListView(
            lista,
            dlg.Corpo.Width,
            dlg.Corpo.Height);

        dlg.AdicionarControle(lvw);

        if (dlg.ShowDialog() != DialogResult.OK)
            return;

        if (lvw.SelectedItems.Count == 0)
            return;

        //Se houver alterações pergunta se deseja salvar
        if (listaFoiModificada && atendimentos.Count > 0)
        {
            var pergunta = Components.CriarDialogBoxConfimacao(
                "Salvar",
                "A lista atual foi modificada. Deseja salvá-la antes de abrir outra?");

            if (pergunta == DialogResult.OK)
                SalvarListaDeAtendimento();
        }

        var caminhoSelecionado = lvw.SelectedItems[0].Tag?.ToString();
        if (string.IsNullOrWhiteSpace(caminhoSelecionado))
            return;
        arquivoAtual = caminhoSelecionado;

        var listaRecuperada = DesserializarArquivo(caminhoSelecionado);

        atendimentos.RaiseListChangedEvents = false;

        atendimentos.Clear();

        foreach (var item in listaRecuperada)
            atendimentos.Add(item);

        atendimentos.RaiseListChangedEvents = true;
        atendimentos.ResetBindings();

        atendimentosData = new AtendimentosData([.. atendimentos]);

        //mantém o mesmo ID do arquivo recuperado
        atendimentosData.SetIdFromFileName(Path.GetFileNameWithoutExtension(caminhoSelecionado)
                                .Replace("atendimento-", ""));

        listaFoiModificada = false;
        listaFoiSalva = true;

        AtualizarProcedimentosGrid();
    }
    private List<Atendimento> DesserializarArquivo(string pathArquivo)
    {
        using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(pathArquivo));
        try
        {
            string jsonAtendimentos = doc.RootElement
                                    .GetProperty("Atendimentos")
                                    .GetRawText();
            //MessageBox.Show(jsonAtendimentos);
            return JsonSerializer.Deserialize<List<Atendimento>>(jsonAtendimentos, jsonOptions) ?? [];
        }
        catch (Exception)
        {
            //MessageBox.Show(ex.ToString());
            throw;
        }

    }
    private Dictionary<string, string> ListaArquivos()
    {
        var pasta = pastaAtendimentos;
        Dictionary<string, string> listArquivos = [];
        var files = Directory.GetFiles(pasta, "*.json");
        var sortedFiles = files.OrderByDescending(f => GerarRex().Replace(f, m => m.Value.PadLeft(10, '0'))); //ordenar números
        foreach (var arquivo in sortedFiles)
        {
            using JsonDocument documento = JsonDocument.Parse(File.ReadAllText(arquivo));

            if (!documento.RootElement.TryGetProperty("Aplicacao", out var app))
                continue;

            if (app.GetString() != "ListarExames")
                continue;
            if (!documento.RootElement.TryGetProperty("Versao", out var ver))
                continue;
            if (ver.GetInt32() != 1)
                continue;
            if (app.GetString() != "ListarExames")
                continue;
            if (!documento.RootElement.TryGetProperty("Id", out var id))
                continue;
            if (string.IsNullOrEmpty(id.GetString()))
                continue;
            listArquivos.Add(id.GetString() ?? "", arquivo);
        }
        return listArquivos;
    }
    private async void BtnAdicionarProcedimento_Click(object? sender, EventArgs e)
    {
        AdicionarLinhaProcedimento(procedimentosCarregados);
    }
    private void BtnCancelarAtendimento_Click(object? sender, EventArgs e)
    {
        var hResposta = Components.CriarDialogBoxConfimacao(
            "Cancelar",
            "Deseja cancelar a inserção do atendimento? os dados inseridos serão perdidos",
            corDaFonte: Color.DarkGoldenrod, estiloDaFonte: FontStyle.Regular);

        if (hResposta == DialogResult.OK)
        {
            if (btnAdicionarAtendimento is not null)
                btnAdicionarAtendimento.Enabled = true;
            DestruirFormularioAdicionarAtendimento();
        }
    }
    private void BtnDefinirPadroes_Click(object? sender, EventArgs e)
    {
        var resultado = Components.CriarDialogDefinicoes(padroesCarregados, procedimentosCarregados, out var saida);
        if (resultado == DialogResult.OK)
        {
            if (saida == null || saida.Length == 0)
                return;
            foreach (var obj in saida)
            {
                if (obj is Padroes padrao)
                {
                    padroesCarregados = padrao;
                    if (cmbMedicos != null)
                    {
                        cmbMedicos.DataSource = padroesCarregados?.Medicos?.ToList();
                        cmbMedicos.CreateControl();
                        cmbMedicos.BeginInvoke(new Action(() => cmbMedicos.SelectedItem = padroesCarregados?.Medicos?.FirstOrDefault()));
                    }
                    SalvarPadroes(padroesCarregados);
                }
                else if (obj is List<Procedimento> procedimentos)
                {
                    procedimentosCarregados = procedimentos;
                    SalvarProcedimentos(procedimentosCarregados);
                    AtualizarDataSourceCombobox(procedimentos);
                }
            }

            Alerta.Show("Salvo", "Definições salvas com sucesso!", AlertaType.Success);
        }
    }
    private void AtualizarDataSourceCombobox(List<Procedimento> dataSource)
    {

        if (flowLayoutPanelProcedimentos is null)
            return;
        var panels = flowLayoutPanelProcedimentos.Controls.OfType<Panel>().ToArray();
        if (panels.Length == 0)
            return;
        var cmbs = panels
            .Select(
                p =>
                p.Controls.OfType<ComboBox>()
                .FirstOrDefault())
            .Where(c => c is not null)
            .ToArray();
        int[] indexes = new int[cmbs.Length];
        var lista = new List<Procedimento> { new("", "Selecione um procedimento") };
        lista.AddRange(dataSource);
        foreach (var cmb in cmbs)
        {
            if (cmb is null)
                continue;
            var valorAntigo = cmb.SelectedValue?.ToString();
            cmb.DataSource = lista.ToList();
            ProcurarProcedimento(lista,  valorAntigo ?? "", out var resultado);
            if (string.IsNullOrEmpty(resultado))
                cmb.SelectedValue = "";
            cmb.SelectedValue = resultado;
        }
    }
    private void ProcurarProcedimento(List<Procedimento> procedimentos, string value, out string resultado)
    {
        var proc = procedimentos?.FirstOrDefault(p => p.Codigo == value);
        if (proc != null)
        {
            resultado = proc.Codigo ?? "";
            return;
        }
        resultado = "";

    }
    private void ButtonPaint(object? sender, PaintEventArgs e)
    {
        if (sender is not Button btn)
            return;

        var corFundo = btn.Parent?.BackColor ?? Color.Black;
        e.Graphics.Clear(corFundo);
        if (btn.BackColor != corFundo)
        {
            using var brushHover = new SolidBrush(btn.BackColor);
            e.Graphics.FillRectangle(brushHover, btn.ClientRectangle);
        }
        if (btn?.Image != null)
        {
            e.Graphics.DrawImage(btn.Image, new Rectangle(0, 0, btn.Width, btn.Height));
        }
    }
    private void ButtonPaint_MouseEnter(object? sender, EventArgs e)
    {
        if (sender is not Button btn)
            return;
        btn.BackColor = Color.FromArgb(40, Color.DodgerBlue);
    }
    private async void CmbProcedimentos_Leave(object? sender, EventArgs e)
    {
        if (sender is not ComboBox cmb)
            return;
        if (cmb == null)
            return;
        if (cmb.SelectedItem is not Procedimento item)//pega o item selecionado
            return;
        if (string.IsNullOrEmpty(cmb.Text) || item is null) //se o texto do combobox está vazio ou não há item selecionado
        {
            if (cmb.Items[0] is not Procedimento primeiroItem)//pega o primeiro item da lista
                return;

            if (primeiroItem == null) //testa se o primeiro procedimento não nulo
                return;

            cmb.SelectedItem = primeiroItem; //seleciona o primeiro item
            cmb.Text = primeiroItem.Nome; //garante que o nome do item selecionado apareça
            return;
        }
        if (!string.IsNullOrEmpty(item.Codigo))//se item selecionado possui código diferente de vazio
        {
            mensagemUI?.MostrarSucesso("Procedimento adicionado com sucesso!");
            if (mensagemUI is not null)
                await mensagemUI.LimparMensagem();
        }
    }
    private async void ChkCpf_Changed(object? sender, EventArgs e)
    {
        if (txtCpfPaciente is null || chkCpf is null || chkCns is null)
            return;
        txtCpfPaciente.Text = "";
        txtCpfPaciente.Enabled = chkCpf.Checked;
        if (!chkCns.Checked && !chkCpf.Checked)
        {
            mensagemUI?.MostrarErro("Ou CNS ou o CPF precisa ser informado!");
        }
    }
    private async void ChkCNS_Changed(object? sender, EventArgs e)
    {
        if (txtCnsPaciente is null || chkCns is null || chkCpf is null)
            return;
        txtCnsPaciente.Text = "";
        txtCnsPaciente.Enabled = chkCns.Checked;
        if (!chkCns.Checked && !chkCpf.Checked)
        {
            mensagemUI?.MostrarErro("Ou CNS ou o CPF precisa ser informado!");
        }

    }
    private async void ChkNome_Changed(object? sender, EventArgs e)
    {
        if (txtNomePaciente is null || chkNome is null)
            return;
        txtNomePaciente.Text = "";
        txtNomePaciente.Enabled = chkNome.Checked;
        mensagemUI?.MostrarInfo("O nome do paciente é opcional, mas ajudará caso ele não esteja cadastrado!");
    }
    private void CmbProcedimento_MouseWheel(object? sender, MouseEventArgs e)
    {
        if (sender is not ComboBox cmb)
            return;
        if (cmb == null)
            return;
        // Só permite o scroll quando a lista estiver aberta
        if (!cmb.DroppedDown)
        {
            ((HandledMouseEventArgs)e).Handled = true;
        }
    }
    //evento para garantir apenas números no textbox (usado em CPF e CNS)
    private void OnlyNumberKeyPress(object? sender, KeyPressEventArgs e)
    {
        if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
        {
            // Bloqueia o caractere impedindo que apareça na TextBox
            e.Handled = true;
        }
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex GerarRex();
}
