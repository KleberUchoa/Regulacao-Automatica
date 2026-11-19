using listar_exames.helpers;
using regulacao.core.models;
using System.ComponentModel;
using System.Text.Encodings.Web;
using System.Text.Json;
using UI;

namespace listar_exames;

public partial class FormPrincipal : Form
{
    #region Configuração Form Principal
    private IContainer? componentsmodel = null;
    private void OnLoad(object? sender, EventArgs e)
    {
        if (!Directory.Exists(pastaDados))
            Directory.CreateDirectory(pastaDados);
        if (!Directory.Exists(pastaAtendimentos))
            Directory.CreateDirectory(pastaAtendimentos);
        procedimentosCarregados = CarregarProcedimentos();
        padroesCarregados = CarregarPadroes();

    }
    private void OnClose(object? sender, EventArgs e)
    {
        ChecarListAtendimentos();
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing && (componentsmodel != null))
        {
            componentsmodel.Dispose();
        }
        base.Dispose(disposing);
    }
    private void InitializeComponent()
    {
        btnAdicionarAtendimento = new Button();

        SuspendLayout(); 
        // FormPrincipal
        // 
        AutoScaleDimensions = new SizeF(6F, 13F);
        AutoScaleMode = AutoScaleMode.Font;
        Font = new Font("Segoe UI", 11, FontStyle.Regular);
        ClientSize = new Size(950, 300);
        Size = new Size(950, 450);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = true;
        Name = "Principal";
        Text = "Listar Exames";
        Load += OnLoad;
        FormClosing += OnClose;
        Icon = AppResources.AppIcon;

        // As imagens são inicializadas em InicializarImagens() antes de InitializeComponent().
    #endregion
        #region Configurando Tela Inicial de Atendimento
        var bounds = Bounds;
        int margem = 10;
        var location = new Point(margem * 2, 40);
        listAtendimentos = CriarDataGridView(location, new Size(bounds.Width / 2, 270));
        listAtendimentos.DataSource = atendimentos;
        atendimentos.ListChanged += Atendimentos_ListChanged;

        location = new Point(listAtendimentos.Width / 2 - 74, 10);
        lblTitulo = Components.CriarLabel(
            location,
            new Font("Segoe UI", 15, FontStyle.Bold),
            "Lista de Atendimentos",
            Color.Black
        );
        lblTitulo.TextAlign = ContentAlignment.MiddleCenter;

        var pnlSize = new Size(495, 100);
        var pnlLocation = new Point(
            ClientRectangle.Right - pnlSize.Width,
            ClientRectangle.Bottom - pnlSize.Height
            );
        pnlMensagem = Components.CriarPanel(
            pnlLocation,
            pnlSize,
            Color.MistyRose,
            false
        );
        lblMensagem = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(8, 0, 0, 0)
        };
        pnlMensagem.Controls.Add(lblMensagem);

        mensagemUI = new MensagemUI(pnlMensagem, lblMensagem);

        var btnSize = 40;
        var btnSpacing = 20;
        var btnY = listAtendimentos.Bottom + btnSpacing;
        var btnX = listAtendimentos.Left + (listAtendimentos.Width / 2) - (btnSize * 3);
        btnLimparLista = Components.CriarButton(
            new Point(btnX, btnY),
            new Size(btnSize, btnSize), 
            AppResources.Trash
        );
        tltAtendimento.SetToolTip(btnLimparLista, "Limpar Lista de Pacientes");
        btnLimparLista.Paint += ButtonPaint;
        btnLimparLista.Click += BtnLimparListaDeAtendimento_Click;
        btnX += btnSize + btnSpacing;
        btnEnviarParaRegulacao = Components.CriarButton(
           new Point(btnX, btnY),
           new Size(btnSize, btnSize),
           AppResources.Play
        );
        tltAtendimento.SetToolTip(btnEnviarParaRegulacao, "Enviar Exames para Regulação");
        btnEnviarParaRegulacao.Paint += ButtonPaint;
        btnEnviarParaRegulacao.Click += BtnEnviarParaRegulacao_Click;

        btnX += btnSize + btnSpacing;
        btnSalvarListaDeAtendimento = Components.CriarButton(
            new Point(btnX, btnY),
            new Size(btnSize, btnSize),
            AppResources.Save
        );
        tltAtendimento.SetToolTip(btnSalvarListaDeAtendimento, "Salvar Lista de Pacientes");
        btnSalvarListaDeAtendimento.Paint += ButtonPaint;
        btnSalvarListaDeAtendimento.Click += BtnSalvarListaDeAtendimento_Click;
        btnX += btnSize + btnSpacing;
        btnRecuperarListaDeAtendimento = Components.CriarButton(
            new Point(btnX, btnY),
            new Size(btnSize, btnSize),
            AppResources.Return
            );
        btnRecuperarListaDeAtendimento.Paint += ButtonPaint;
        tltAtendimento.SetToolTip(btnRecuperarListaDeAtendimento, "Recuperar Lista de Pacientes");
        btnRecuperarListaDeAtendimento.Click += BtnRecuperarAtendimento_Click;

        Controls.Add(lblTitulo);
        Controls.Add(listAtendimentos);
        Controls.Add(btnLimparLista);
        Controls.Add(btnEnviarParaRegulacao);
        Controls.Add(btnSalvarListaDeAtendimento);
        Controls.Add(btnRecuperarListaDeAtendimento);
        Controls.Add(pnlMensagem);

        location = new Point(Width - 60, lblTitulo.Top);
        btnAdicionarAtendimento = Components.CriarButton(
            location,
            new Size(30, 30),
            AppResources.Plus
        );
        tltAtendimento.SetToolTip(btnAdicionarAtendimento, "Adicionar Atendimento");

        //Emula a transparência
        btnAdicionarAtendimento.Paint += ButtonPaint;
        btnAdicionarAtendimento.Click += BtnAdicionarAtendimento_Click;

        location = new Point(btnAdicionarAtendimento.Left - 30, btnAdicionarAtendimento.Top);
        btnDefinirPadroes = Components.CriarButton(
            location,
            new Size(30, 30),
            AppResources.Config
        );
        tltAtendimento.SetToolTip(btnDefinirPadroes, "Definir Padrões");
        btnDefinirPadroes.Paint += ButtonPaint;
        btnDefinirPadroes.Click += BtnDefinirPadroes_Click;

        Controls.Add(btnDefinirPadroes);
        Controls.Add(btnAdicionarAtendimento);
        ResumeLayout(false);
    }
    private void AbrirFormularioAdicionarAtendimento(int? atendimentoId = null, string? nomePaciente = null, string? cpf = null,
    string? cns = null, DateTime? dataExame = null, string? historico = null, string? medicoSolicitante = null, List<Procedimento>? procedimentos = null)
    {
        int margem = 10;
        
        var gradeAtendimentos = listAtendimentos ?? throw new InvalidOperationException("A grade de atendimentos ainda não foi inicializada.");
        var botaoAdicionar = btnAdicionarAtendimento ?? throw new InvalidOperationException("O botão de adicionar ainda não foi inicializado.");
        var padroes = padroesCarregados ?? new PadroesData().padroes;
        //DestruirFormularioAdicionarAtendimento();
        
        int formWidth = Bounds.Width / 2 - 10 * margem;
        int formHeight = gradeAtendimentos.Height;

        groupFormularioAtendimento = Components.CriarGroupBox(
            new Point(gradeAtendimentos.Right + 3 * margem, botaoAdicionar.Bottom),
            new Size(formWidth, formHeight),
            "Atendimento"
        );
        int oneColumnWidth = formWidth - 4 * margem;
        txtNomePaciente = Components.CriarTextBox(
            new Point(margem, 30),
            new Size(oneColumnWidth, 20),
            "Nome do Paciente",
            text: nomePaciente,
            casing: CharacterCasing.Upper
        );
        txtNomePaciente.Font = fonte;

        chkNome = Components.CriarCheckBox(new Point(txtNomePaciente.Right + margem, txtNomePaciente.Top));
        chkNome.CheckedChanged += ChkNome_Changed;
        tltAtendimento.SetToolTip(chkNome, "Desabilitar Nome do Paciente");
        
        int twoColumnsWidth = formWidth / 2 - 4 * margem; //metade da largura do formulário menos margens

        txtCpfPaciente = Components.CriarTextBox(
            new Point(margem, txtNomePaciente.Bottom + margem),
            new Size(twoColumnsWidth, 20),
            "CPF do Paciente",
            text: cpf,
            maxLeght: 11
        );
        txtCpfPaciente.KeyPress += OnlyNumberKeyPress;
        txtCpfPaciente.Font = fonte;
        chkCpf = Components.CriarCheckBox(new Point(txtCpfPaciente.Right + margem, txtCpfPaciente.Top));
        chkCpf.CheckedChanged += ChkCpf_Changed;
        tltAtendimento.SetToolTip(chkCpf, "Desabilitar CPF");

        txtCnsPaciente = Components.CriarTextBox(
            new Point(txtNomePaciente.Right - twoColumnsWidth, txtNomePaciente.Bottom + margem),
            new Size(twoColumnsWidth, 20),
            "CNS do Paciente",
            text: cns,
            maxLeght: 15
        );
        txtCnsPaciente.KeyPress += OnlyNumberKeyPress;
        txtCnsPaciente.Font = fonte;
        chkCns = Components.CriarCheckBox(new Point(txtCnsPaciente.Right + margem, txtCnsPaciente.Top));
        chkCns.CheckedChanged += ChkCNS_Changed;
        tltAtendimento.SetToolTip(chkCns, "Desabilitar CNS");

        dtpDataExame = Components.CriarDateTimePicker(
            new Point(margem, txtCpfPaciente.Bottom + margem),
            new Size(twoColumnsWidth, 20)
        );
        dtpDataExame.Font = fonte;
        cmbMedicos = new ComboBox
        {
            Location = new Point(oneColumnWidth - twoColumnsWidth + margem, dtpDataExame.Top),
            Size = new Size(twoColumnsWidth, 30),
            DataSource = padroes.Medicos?.ToList() ?? [],
            Font = fonte
        };
        cmbMedicos.CreateControl();
        cmbMedicos.BeginInvoke(new Action(() =>
        {
            cmbMedicos.SelectedIndex = 0;
        }));
        txtHistorico = Components.CriarTextBox(
            new Point(margem, dtpDataExame.Bottom + margem),
            new Size(oneColumnWidth, 30),
            "Histórico Clínico"
        );
        txtHistorico.Font = fonte;
        txtId = Components.CriarTextBox(
            new Point(txtHistorico.Right + margem, txtHistorico.Top),
            new Size(10, 10),
            ""
        );
        txtId.Enabled = false;
        txtId.Visible = false;
        btnAdicionarAtendimento.Enabled = false; // Desabilita o botão "Adicionar Atendimento" enquanto o formulário está aberto

        btnAdicionarProcedimento = Components.CriarButton(
            new Point(4, 5),
            new Size(25, 25),
           AppResources.Plus
        );
        tltAtendimento.SetToolTip(btnAdicionarProcedimento, "Acrescentar Procedimento");

        //Emula a transparência
        btnAdicionarProcedimento.Paint += ButtonPaint;
        btnAdicionarProcedimento.Click += BtnAdicionarProcedimento_Click;
        int painelWidth = (int)(oneColumnWidth - 1.5 * margem);
        Panel panelAddProc = new()
        {
            Width = painelWidth,
            Height = 35,
            Location = new Point(margem, txtHistorico.Bottom + margem)
        };
        panelAddProc.Controls.Add(btnAdicionarProcedimento);


        flowLayoutPanelProcedimentos = Components.CriarFlowLayoutPanel(
            new Point(margem, panelAddProc.Bottom + margem),
            new Size(painelWidth, groupFormularioAtendimento.Height - panelAddProc.Bottom - 2 * margem)
        );
        flowLayoutPanelProcedimentos.Font = fonte;

        btnCancelarAtendimento = Components.CriarButton(
            new Point(flowLayoutPanelProcedimentos.Right + margem, flowLayoutPanelProcedimentos.Top),
            new Size(25, 25),
            AppResources.Minus
        );
        tltAtendimento.SetToolTip(btnCancelarAtendimento, "Cancelar Atendimento");
        btnCancelarAtendimento.Click += BtnCancelarAtendimento_Click;
        //Emula a transparência
        btnCancelarAtendimento.Paint += ButtonPaint;

        btnSalvarAtendimento = Components.CriarButton(
            new Point(flowLayoutPanelProcedimentos.Right + margem, btnCancelarAtendimento.Bottom + 2 * margem),
            new Size(25, 25),
            AppResources.Save
        );
        tltAtendimento.SetToolTip(btnSalvarAtendimento, "Salvar Atendimento");
        btnSalvarAtendimento.Click += BtnSalvarAtendimento_Click;
        //Emula a transparência
        btnSalvarAtendimento.Paint += ButtonPaint;

        //Inicializar caso não nulo (modo de edição)
        if (editMode)
        {
            txtId.Text = (atendimentoId.HasValue) ? atendimentoId.ToString() : txtId.Text;
            if (!string.IsNullOrEmpty(medicoSolicitante))
            {
                cmbMedicos.DataSource = padroes.Medicos?.ToList() ?? [];
                cmbMedicos.CreateControl();
                cmbMedicos.BeginInvoke(new Action(() =>
                {
                    cmbMedicos.SelectedItem = medicoSolicitante;
                }));
            }
            if (!string.IsNullOrEmpty(nomePaciente))
            {
                txtNomePaciente.Text = nomePaciente;
            }
            else
            {
                chkNome.Checked = false;
            }
            if (!string.IsNullOrEmpty(cpf))
            {
                txtCpfPaciente.Text = cpf;
            }
            else
            {
                chkCpf.Checked = false;
            }
            if (!string.IsNullOrEmpty(cns))
            {
                txtCnsPaciente.Text = cns;
            }
            else
            {
                chkCns.Checked = false;
            }
            txtHistorico.Text = (!string.IsNullOrEmpty(historico)) ? historico : txtHistorico.Text;
            dtpDataExame.Value = dataExame.HasValue ? (DateTime)dataExame : dtpDataExame.Value;
        }
        if (procedimentos != null)
        {
            foreach (var procs in procedimentos)
            {
                AdicionarLinhaProcedimento(procedimentosCarregados, procs.Codigo ?? string.Empty);
            }
        }
        else
            AdicionarLinhaProcedimento(procedimentosCarregados); //adiciona novo combobox e carrega lista de procedimentos

        groupFormularioAtendimento.Controls.AddRange
        (
            txtNomePaciente,
            txtCpfPaciente,
            txtCnsPaciente,
            dtpDataExame,
            cmbMedicos,
            txtHistorico,
            flowLayoutPanelProcedimentos,
            btnSalvarAtendimento,
            btnCancelarAtendimento,
            chkCpf,
            chkCns,
            chkNome,
            txtId,
            panelAddProc
        );
        Controls.Add(groupFormularioAtendimento);
    }
    private void AdicionarLinhaProcedimento(List<Procedimento>? procedimentos, string? codProcSelecionado = null)
    {
        Panel painel = new()
        {
            Width = 540,
            Height = 35,
            Margin = new Padding(0, 2, 0, 2)
        };
        //Adiciona um procedimento "vazio inicial"
        var lista = new List<Procedimento>
        {
            new ("", "Selecione um procedimento")
        };
        lista.AddRange(procedimentos ?? []);

        ComboBox cmbProcedimento = new()
        {
            Name = "cmbProcedimento",
            Width = 460,
            Left = 5,
            Top = 4,
            DropDownStyle = ComboBoxStyle.DropDown,
            AutoCompleteMode = AutoCompleteMode.SuggestAppend,
            AutoCompleteSource = AutoCompleteSource.ListItems,
            DataSource = lista.ToList(),
            DisplayMember = "Nome",
            ValueMember = "Codigo",
        };
        cmbProcedimento.Leave += CmbProcedimentos_Leave;
        cmbProcedimento.MouseWheel += CmbProcedimento_MouseWheel;
        cmbProcedimento.KeyPress += (s, e) => { e.KeyChar = char.ToUpper(e.KeyChar); };
        var btnExcluir = new Button
        {
            Text = "🗑️",
            Width = 30,
            Height = 30,
            Left = cmbProcedimento.Right + 5,
            Top = 2
        };
        Components.ConfigurarHover(btnExcluir);
        btnExcluir.Click += (s, e) =>
        {
            flowLayoutPanelProcedimentos?.Controls.Remove(painel);
            painel.Dispose();
        };
        painel.Controls.Add(cmbProcedimento);
        painel.Controls.Add(btnExcluir);

        flowLayoutPanelProcedimentos?.Controls.Add(painel);

        cmbProcedimento.CreateControl(); //para poder chamar o begininvoke
        if (!string.IsNullOrEmpty(codProcSelecionado))
        {
            cmbProcedimento.BeginInvoke(new Action(() =>
            {
                cmbProcedimento.SelectedValue = codProcSelecionado;
            }));
        }
        else
        {
            cmbProcedimento.BeginInvoke(new Action(() =>
            {
                cmbProcedimento.SelectedIndex = 0;
            }));
        }

        cmbProcedimento.Focus();
       cmbProcedimento.SelectAll();
    }
    private List<Procedimento> CarregarProcedimentos()
    {
        List<Procedimento> procedimentos = new();
        var path = Path.Combine(pastaDados, "Procedimentos.json");
        if (!File.Exists(path))
        {
            try
            {
                var procedimentosData = new ProcedimentosData();
                procedimentosData.Procedimentos = [..procedimentosData.Procedimentos.OrderBy(p => p?.Nome?.ToUpper())];
                SalvarProcedimentos([.. procedimentosData.Procedimentos]);
            }
            catch (Exception ex)
            {
                Alerta.Show("Erro", $"Erro ao salvar procedimentos: {ex.Message}", AlertaType.Error);
            }
        }
        try
        {
            var jsonProcedimentos = File.ReadAllText(path);
            var dados = JsonSerializer.Deserialize<List<Procedimento>>(jsonProcedimentos, jsonOptions);

            if (dados == null)
                throw new Exception();

            procedimentos = dados;
        }
        catch (Exception ex)
        {
            Alerta.Show("Erro", $"Erro ao carregar procedimentos: {ex.Message}", AlertaType.Error);
        }

        return procedimentos;
    }
    private Padroes CarregarPadroes()
    {
        var path = Path.Combine(pastaDados, "Padroes.json");
        bool arquivoValido = true;
        if(!File.Exists(path))
        {
            try
            {
                PadroesData data = new();
                SalvarPadroes(data.padroes);
            }
            catch(Exception)
            {
                Alerta.Show("Erro","Não foi possível gravar o arquivo", AlertaType.Error);
            }
        }
        using JsonDocument json = JsonDocument.Parse(File.ReadAllText(path));
        if(!json.RootElement.TryGetProperty("Site", out var site))
            arquivoValido = false;
        if(!json.RootElement.TryGetProperty("Usuario", out var email))
            arquivoValido = false;
        if (!json.RootElement.TryGetProperty("Senha", out var senha))
            arquivoValido = false;
        if (!json.RootElement.TryGetProperty("EstabelecimentoRequisitado", out var estabelecimentoRequisitado))
            arquivoValido = false;
        if (!json.RootElement.TryGetProperty("Medicos", out var medicos))
            arquivoValido = false;
        if (!json.RootElement.TryGetProperty("Cid", out var cid))
            arquivoValido = false;
        if (!json.RootElement.TryGetProperty("Prioridade", out var prioridade))
            arquivoValido = false;
        if (!json.RootElement.TryGetProperty("NivelDeUrgencia", out var nivelDeUrgencia))
            arquivoValido = false;
        if (!arquivoValido)
        {
            PadroesData data = new();
            SalvarPadroes(data.padroes);
            Alerta.Show("Erro", "Arquivo de padrões inválido, os padrões foram resetados", AlertaType.Error);
            return data.padroes;
        }
        List<string> listaMedicos = [.. medicos.EnumerateArray().Select(e => e.GetString() ?? string.Empty)];
        
        return new Padroes(site.GetString() ?? string.Empty, 
            email.GetString() ?? string.Empty,
            string.IsNullOrEmpty(senha.GetString()) ? "" : ProtegerHelper.Desproteger(senha.GetString() ?? string.Empty),
            estabelecimentoRequisitado.GetString() ?? string.Empty,
            [.. listaMedicos],
            cid: cid.GetString() ?? string.Empty,
            prioridade: prioridade.GetString() ?? string.Empty,
            nivelUrgencia: nivelDeUrgencia.GetString() ?? string.Empty);
    }
    private void SalvarPadroes(Padroes? padroes)
    {
        if (padroes is null)
        {
            Alerta.Show("Erro", "Não há definições para gravar.", AlertaType.Error);
            return;
        }

        var padroesProtegidos = new Padroes(
            padroes.Site, padroes.Usuario,
            ProtegerHelper.Proteger(padroes.Senha),
            padroes.EstabelecimentoRequisitado,
            padroes.Medicos, padroes.Cid,
            padroes.Prioridade, padroes.NivelDeUrgencia
            );
        var path = Path.Combine(pastaDados, "Padroes.json");
        var json = JsonSerializer.Serialize(padroesProtegidos, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
        if (json == null)
        {
            Alerta.Show("Erro", "Procedimentos não serializados", AlertaType.Error);
            return;
        }
        try
        {
            File.WriteAllText(path, json);
        }
        catch
        {
            Alerta.Show("Erro", "Não foi possível escrever o arquivo", AlertaType.Error);
        }
    }
    private void ChecarListAtendimentos()
    {
        if (listAtendimentos is not null && listAtendimentos.RowCount != 0 && !listaFoiSalva)
        {
            var hResposta = Components.CriarDialogBoxConfimacao(
                "Salvar", 
                "Há dados não salvos na lista de atendimentos, deseja salvá-los?",
                largura: 500, altura: 400
            );
            if (hResposta == DialogResult.OK)
            {
                SalvarListaDeAtendimento();
            }
        }
    }
    private Atendimento CriarAtendimento()
    {
        var painelProcedimentos = flowLayoutPanelProcedimentos ?? throw new InvalidOperationException("O formulário de atendimento não está aberto.");
        var dataExameControl = dtpDataExame ?? throw new InvalidOperationException("A data do exame não foi preenchida.");
        var comboMedicos = cmbMedicos ?? throw new InvalidOperationException("O médico solicitante não foi selecionado.");

        List<Procedimento> procs = new();

        foreach (Panel pnl in painelProcedimentos.Controls.OfType<Panel>())
        {
            var cmb = pnl.Controls
                            .OfType<ComboBox>()
                            .FirstOrDefault();
            if (cmb == null)
                continue;
            var proc = cmb.SelectedItem as Procedimento;
            if (proc != null && !string.IsNullOrEmpty(proc.Codigo))
            {
                procs.Add(proc);
            }
        }
        int id;

        if (editMode)
        {
            id = Convert.ToInt32(txtId?.Text);
        }
        else
        {
            id = proximoId++;
        }
        return new Atendimento(
             id,
             txtNomePaciente?.Text,
             txtCpfPaciente?.Text,
             txtCnsPaciente?.Text,
             dataExameControl.Value.Date,
             txtHistorico?.Text,
             comboMedicos.SelectedItem?.ToString(),
             procs
             );
    }
    private void DestruirFormularioAdicionarAtendimento()
    {
        if (groupFormularioAtendimento != null)
        {
            Controls.Remove(groupFormularioAtendimento);
            groupFormularioAtendimento.Dispose();
            groupFormularioAtendimento = null;
            //limpa controles
            txtNomePaciente = null;
            chkNome = null;
            chkCns = null;
            txtCpfPaciente = null;
            chkCpf = null;
            txtCnsPaciente = null;
            txtHistorico = null;
            dtpDataExame = null;
            btnAdicionarProcedimento = null;
            btnSalvarAtendimento = null;
            btnCancelarAtendimento = null;
            flowLayoutPanelProcedimentos = null;
        }
    }
    private void ReconstruirFormularioAtendimento(Atendimento? atendimento)
    {
        DestruirFormularioAdicionarAtendimento();
        AbrirFormularioAdicionarAtendimento(
            atendimento?.Id,
            atendimento?.Nome,
            atendimento?.Cpf,
            atendimento?.Cns,
            atendimento?.DataAtendimento,
            atendimento?.Historico,
            atendimento?.MedicoSolicitante,
            atendimento?.Procedimentos ?? new());
    }
    private void SalvarProcedimentos(List<Procedimento> procedimentos)
    {
        procedimentos = [..procedimentos.OrderBy(p=> p.Nome)];
        var path = Path.Combine(pastaDados, "Procedimentos.json");
        if (procedimentos == null)
        {
            Alerta.Show("Carregado", "Não foi possível gravar o arquivo", AlertaType.Success);
            return;
        }
        var json = JsonSerializer.Serialize(procedimentos, jsonOptions);
        if (json == null)
        {
            Alerta.Show("Erro", "Procedimentos não serializados", AlertaType.Error);
            return;
        }
        try
        {
            File.WriteAllText(path, json);
        }
        catch
        {
            Alerta.Show("Erro", "Não foi possível escrever o arquivo", AlertaType.Error);
        }
    }
    #endregion
    #region  Váriaveis Globais
    private Button? btnLimparLista;
    private Button? btnAdicionarAtendimento;
    private Button? btnSalvarAtendimento;
    private Button? btnAdicionarProcedimento;
    private Button? btnCancelarAtendimento;
    private Button? btnEnviarParaRegulacao;
    private Button? btnSalvarListaDeAtendimento;
    private Button? btnRecuperarListaDeAtendimento;
    private Button? btnDefinirPadroes;
    private CheckBox? chkCpf;
    private CheckBox? chkCns;
    private CheckBox? chkNome;
    private ComboBox? cmbMedicos;
    private DataGridView? listAtendimentos;
    private DateTimePicker? dtpDataExame;
    private FlowLayoutPanel? flowLayoutPanelProcedimentos;
    private GroupBox? groupFormularioAtendimento;
    private Panel? pnlMensagem;
    private Label? lblMensagem;
    private Label? lblTitulo;
    private MensagemUI? mensagemUI;
    private TextBox? txtNomePaciente;
    private TextBox? txtCpfPaciente;
    private TextBox? txtCnsPaciente;
    private TextBox? txtHistorico;
    private TextBox? txtId;
    private readonly ToolTip tltAtendimento = new();
    private bool editMode = false;
    private int proximoId = 1;
    List<Procedimento>? procedimentosCarregados;
    private Padroes? padroesCarregados;
    //Salvar na pasta AppData do usuário 
    private readonly string pastaDados = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Listar Exames");
    private readonly string pastaAtendimentos = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Listar Exames", "atendimentos");
    private string? arquivoAtual;
    private bool listaFoiModificada; //se bindinglist atendimentos foi modificado
    private bool listaFoiSalva; //se bindinglist atendimentos foi salva
    private BindingList<Atendimento> atendimentos = new();
    private AtendimentosData atendimentosData = new();
    private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    private readonly Font fonte = new("Segoe UI", 9);
    #endregion
}
