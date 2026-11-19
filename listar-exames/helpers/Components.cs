using regulacao.core.models;
using System.Diagnostics;
using UI;
namespace listar_exames.helpers;

public static class Components
{
    private static readonly Font fonte = new("Segoe UI", 7, FontStyle.Regular);
    public static TextBox CriarTextBox(Point location, Size size, string? placeHolder = null, string? text = null, CharacterCasing? casing = null, int? maxLeght = null)
    {
        var txt = new TextBox
        {
            Location = location,
            Size = size,
            PlaceholderText = placeHolder,
            Text = text,
            CharacterCasing = casing ?? CharacterCasing.Normal
        };
        txt.MaxLength = maxLeght ?? txt.MaxLength;
        return txt;
    }
    public static Label CriarLabel(Point location, Font font, string text, Color foreColor)
    {
        return new Label()
        {
            Location = location,
            AutoSize = true,
            Font = font,
            Text = text,
            ForeColor = foreColor
        };
    }
    public static Button CriarButton(Point location, Size size, Image? imagem = null)
    {
        var btn = new Button()
        {
            Location = location,
            Size = size,
            FlatStyle = FlatStyle.Flat,
            Image = imagem
        };
        btn.FlatAppearance.BorderSize = 0;
        ConfigurarHover(btn);
        return btn;
    }

    public static void ConfigurarHover(Button btn)
    {
        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, Color.DodgerBlue);
        btn.ParentChanged += (_, _) =>
        {
            if (btn.Parent != null)
                btn.BackColor = btn.Parent.BackColor;
        };
        btn.MouseEnter += (_, _) =>
        {
            btn.BackColor = Color.FromArgb(40, Color.DodgerBlue);
            btn.Invalidate();
        };
        btn.MouseLeave += (_, _) =>
        {
            btn.BackColor = btn.Parent?.BackColor ?? SystemColors.Control;
            btn.Invalidate();
        };
    }
    public static CheckBox CriarCheckBox(Point location)
    {
        return new CheckBox
        {
            Location = location,
            Checked = true,
            AutoSize = true,
        };

    }
    public static GroupBox CriarGroupBox(Point location, Size size, string text)
    {
        return new GroupBox
        {
            Text = text,
            Size = size,
            Location = location
        };

    }
    public static DateTimePicker CriarDateTimePicker(Point location, Size size)
    {
        return new DateTimePicker
        {
            Location = location,
            Size = size
        };
    }
    public static FlowLayoutPanel CriarFlowLayoutPanel(Point location, Size size)
    {
        return new FlowLayoutPanel
        {
            Location = location,
            Size = size,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BorderStyle = BorderStyle.FixedSingle,
        };
    }
    public static Panel CriarPanel(Point location, Size size, Color backColor, bool visible = true)
    {
        return new Panel
        {
            Location = location,
            Size = size,
            BackColor = backColor,
            Visible = visible,
            //Padding = new Padding(2)
        };
    }
    public static DialogResult CriarDialogBoxConfimacao(
          string titulo, string mensagem, int largura = 450, int altura = 300,
          Image? imgSalvar = null, Image? imgCancelar = null,
          Image? imgFechar = null, FontStyle? estiloDaFonte = null, Color? corDaFonte = null)
    {

        var dialog = new DialogUI(titulo, largura, altura, imgSalvar ?? AppResources.Check,
            imgCancelar ?? AppResources.Minus, imgFechar ?? AppResources.Cross);
        dialog.AdicionarControle(new Label
        {
            Text = mensagem,
            Font = new Font("Segoe UI", 10, estiloDaFonte ?? FontStyle.Bold),
            ForeColor = corDaFonte ?? Color.Red,
            Width = dialog.Corpo.Width - 20,
            Height = dialog.Corpo.Height - 20
        });
        return dialog.ShowDialog();
    }
    public static DialogResult CriarDialogDefinicoes(Padroes? padroes, List<Procedimento>? procedimentos, out object[] saida)
    {
        var padroesExibidos = padroes ?? new PadroesData().padroes;
        var procedimentosExibidos = procedimentos ?? [];
        var dlg = new DialogUI("Definições de Entrada", 700, 550, AppResources.Check, AppResources.Minus, AppResources.Cross);
        var tabControlDefinicoes = new TabControl
        {
            Dock = DockStyle.Fill,
            TabIndex = 0,
            Location = new Point(0, dlg.Corpo.Top),
        };
        var tabPageDefinicoes = new TabPage
        {
            Text = "Definições",
        };
        var tabPageProcedimentos = new TabPage
        {
            Text = "Procedimentos",
        };
        var controlesDefinicoes = CriarControlesDefinicoes(padroesExibidos);
        var controlesProcedimentos = CriarControlesProcedimentos(procedimentosExibidos);

        tabPageDefinicoes.Controls.AddRange(controlesDefinicoes);
        tabPageProcedimentos.Controls.AddRange(controlesProcedimentos);

        tabControlDefinicoes.Controls.Add(tabPageDefinicoes);
        tabControlDefinicoes.Controls.Add(tabPageProcedimentos);

        dlg.AdicionarControles(
            tabControlDefinicoes
        );
        var resultado = dlg.ShowDialog();


        saida = new object[2];
        TextBox[] textboxDefinicoes = [.. tabPageDefinicoes.Controls.OfType<TextBox>()];
        FlowLayoutPanel panelMedicos = tabPageDefinicoes.Controls.OfType<FlowLayoutPanel>().FirstOrDefault() ?? new FlowLayoutPanel();
        var panels = panelMedicos.Controls.OfType<Panel>();
        TextBox[] textboxMedicos = [.. panels.SelectMany(p => p.Controls.OfType<TextBox>())];
        int index = 0;
        if (DefinicoesAlteradas(padroes, textboxDefinicoes, textboxMedicos))
        {
            var medicos = PercorrerTextboxArray(textboxMedicos);
            saida[index++] = new Padroes
            (
                textboxDefinicoes[0].Text, // Site
                textboxDefinicoes[1].Text, // Usuario
                textboxDefinicoes[2].Text, // Senha
                textboxDefinicoes[3].Text, // EstabelecimentoRequisitado
                [.. medicos], // MedicoSolicitante
                textboxDefinicoes[4].Text, // Cid
                textboxDefinicoes[5].Text, // Prioridade
                textboxDefinicoes[6].Text  // NivelDeUrgencia
            );
        }
        var flowLayoutPanels = tabPageProcedimentos.Controls.OfType<FlowLayoutPanel>().FirstOrDefault();
        var panelsProcedimentos = flowLayoutPanels?.Controls.OfType<Panel>().ToArray() ?? [];
        var textBoxesProcedimentos = panelsProcedimentos.SelectMany(panel => panel.Controls.OfType<TextBox>()).ToArray();
        var procedimentosSaida = new List<Procedimento>();
        //pegando os valores dos textboxes de procedimentos e criando uma lista de Procedimento
        DeletarPanelsVazios(flowLayoutPanels ?? new FlowLayoutPanel());
        procedimentosSaida.AddRange(
            textBoxesProcedimentos.Select((tb, i) => new { TextBox = tb, Index = i })
            .Where(x => x.Index % 2 == 0)
            .Select(x => new Procedimento
            (
                textBoxesProcedimentos[x.Index].Text, // Codigo
                textBoxesProcedimentos[x.Index + 1].Text // Nome
            )));
        if (ProcedimentosAlterados(procedimentos, textBoxesProcedimentos))
        {
            saida[index] = procedimentosSaida;
        }
        return resultado;
    }
    private static string[] PercorrerTextboxArray(TextBox[] textBoxes)
    {
        return [..textBoxes
            .Select(tb => tb.Text)
            .Where(t => !string.IsNullOrWhiteSpace(t))];
    }
    private static void DeletarPanelsVazios(FlowLayoutPanel flowLayoutPanel)
    {
        if (flowLayoutPanel == null) return;
        var panelsVazios = flowLayoutPanel.Controls.OfType<Panel>()
            .Where(panel => panel.Controls.OfType<TextBox>().All(tb => string.IsNullOrWhiteSpace(tb.Text)))
            .ToList();
        foreach (var panel in panelsVazios)
        {
            flowLayoutPanel.Controls.Remove(panel);
            panel.Dispose();
        }
    }
    private static bool ProcedimentosAlterados(List<Procedimento>? procedimentos, TextBox[] textboxProcedimentos)
    {
        if (procedimentos == null) return true;
        if (procedimentos.Count != textboxProcedimentos.Length / 2)
        {
            return true;
        }
        for (int i = 0; i < procedimentos.Count; i++)
        {
            if (procedimentos[i].Codigo != textboxProcedimentos[i * 2].Text || //i * 2 é o textbox do código, i * 2 + 1 é o textbox do nome
               procedimentos[i].Nome != textboxProcedimentos[i * 2 + 1].Text)
            {
                return true;
            }
        }
        return false;
    }
    private static bool DefinicoesAlteradas(Padroes? padroes, TextBox[] textboxDefinicoes, TextBox[] textboxMedicos)
    {
        if (padroes?.Site != textboxDefinicoes[0].Text ||
             padroes?.Usuario != textboxDefinicoes[1].Text ||
             padroes?.Senha != textboxDefinicoes[2].Text ||
             padroes?.EstabelecimentoRequisitado != textboxDefinicoes[3].Text ||
             padroes?.Cid != textboxDefinicoes[4].Text ||
             padroes?.Prioridade != textboxDefinicoes[5].Text ||
             padroes?.NivelDeUrgencia != textboxDefinicoes[6].Text) { return true; }
        var medicos = PercorrerTextboxArray(textboxMedicos);

        if (!(padroes?.Medicos ?? []).SequenceEqual(medicos))
            return true;
        return false;
    }
    private static Control[] CriarControlesProcedimentos(List<Procedimento>? procedimentos)
    {
        var toolTip = new ToolTip();
        var location = new Point(10, 10);

        var btnAdicionar = CriarButton(Point.Empty, new Size(40, 40));
        btnAdicionar.Image = AppResources.Plus;
        btnAdicionar.Paint += (s, e) =>
        {
            if (s is not Button btn)
                return;

            if (btn is null)
                return;

            e.Graphics.Clear(btn?.Parent?.BackColor ?? Color.Black);
            if (btn?.Image != null)
            {
                e.Graphics.DrawImage(btn.Image, new Rectangle(0, 0, btn.Width, btn.Height));
            }
        };
        toolTip.SetToolTip(btnAdicionar, "Adicionar Procedimento");
        location.Y += 40;
        var flowLayoutPanel = CriarFlowLayoutPanel(location, new Size(600, 400));
        flowLayoutPanel.Dock = DockStyle.Fill;
        btnAdicionar.Click += (_, _) =>
        {
            var panelProcedimento = AdicionarPanelProcedimento(flowLayoutPanel);
            flowLayoutPanel.Controls.Add(panelProcedimento);
            flowLayoutPanel.ScrollControlIntoView(panelProcedimento);
        };
        var controles = new List<Control> { btnAdicionar, flowLayoutPanel };
        flowLayoutPanel.SuspendLayout();
        foreach (var procedimento in procedimentos ?? [])
        {
            var panelProcedimento = AdicionarPanelProcedimento(flowLayoutPanel, procedimento);
            flowLayoutPanel.Controls.Add(panelProcedimento);
        }
        flowLayoutPanel.ResumeLayout();
        btnAdicionar.Location = new Point(444, 10);

        return [.. controles];
    }
    private static Panel AdicionarPanelProcedimento(FlowLayoutPanel flowLayoutPanel, Procedimento? procedimento = null)
    {
        var toolTip = new ToolTip();
        Font fonte = new("Segoe UI", 7, FontStyle.Regular);
        var espacoHorizontal = 8;
        var espacoVertical = 0;
        var panelProcedimento = CriarPanel(Point.Empty,
                new Size(560, 28), flowLayoutPanel.BackColor);
        var lblCodigo = CriarLabel(new Point(0, 0), fonte, "Código", Color.Black);
        lblCodigo.AutoSize = false;
        lblCodigo.Size = new Size(80, 12);
        lblCodigo.TextAlign = ContentAlignment.TopLeft;
        var txtCodigo = CriarTextBox(new Point(0, lblCodigo.Bottom + espacoVertical),
            new Size(80, 20), text: procedimento?.Codigo ?? "");
        txtCodigo.Font = fonte;
        txtCodigo.KeyPress += (s, e) =>
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        };
        var lblNome = CriarLabel(new Point(txtCodigo.Right + espacoHorizontal, 0),
            fonte, "Procedimento", Color.Black);
        lblNome.AutoSize = true;
        lblNome.TextAlign = ContentAlignment.TopLeft;
        lblNome.AutoSize = false;
        lblNome.Size = new Size(80, 12);
        var txtNome = CriarTextBox(new Point(lblNome.Left, lblNome.Bottom + espacoVertical),
            new Size(260, 20), text: procedimento?.Nome?.ToUpper() ?? "", casing: CharacterCasing.Upper);
        txtNome.Font = fonte;

        var btn = CriarButton(new Point(txtNome.Right + espacoHorizontal, txtNome.Top + 2),
            new Size(25, 25));
        btn.Text = "➖";
        toolTip.SetToolTip(btn, "Remover Procedimento");
        panelProcedimento.Controls.AddRange([lblCodigo, txtCodigo, lblNome, txtNome, btn]);
        btn.Click += (_, _) =>
        {
            flowLayoutPanel.Controls.Remove(panelProcedimento);
            panelProcedimento.Dispose();
        };
        return panelProcedimento;
    }
    private static Control[] CriarControlesDefinicoes(Padroes? padroes)
    {
        var valores = padroes ?? new PadroesData().padroes;
        var pos = new Point(10, 10);
        var (lblCaminho, txtCaminho) = CriarCampo(ref pos, "Caminho do Site", valores.Site);
        var (lblEmail, txtEmail) = CriarCampo(ref pos, "Usuário", valores.Usuario);
        var (lblSenha, txtSenha) = CriarCampo(ref pos, "Senha", valores.Senha);
        txtSenha.UseSystemPasswordChar = true;
        var (lblEstabelecimento, txtEstabelecimento) = CriarCampo(ref pos, "Estabelecimento Requisitado", valores.EstabelecimentoRequisitado.ToUpper());

        var (lblCid, txtCid) = CriarCampo(ref pos, "CID", valores.Cid);
        var (lblPrioridade, txtPrioridade) = CriarCampo(ref pos, "Prioridade", valores.Prioridade.ToUpper());
        var (lblUrgencia, txtUrgencia) = CriarCampo(ref pos, "Nível de Urgência", valores.NivelDeUrgencia.ToUpper());
        var localPanel = new Point(txtCaminho.Top, txtCaminho.Right + 10);
        var sizePanel = new Size(txtCaminho.Width + 20, txtCaminho.Height * 10);
        var panelMedicos = CriarFlowLayoutPanel(localPanel, sizePanel);
        panelMedicos.Dock = DockStyle.Right;
        var panelCabecalho = CriarPanel(Point.Empty, new Size(panelMedicos.Width - 20, 30), panelMedicos.BackColor);
        var lblMedico = new Label
        {
            Text = "Medicos",

        };
        panelCabecalho.Controls.Add(lblMedico);
        panelMedicos.Controls.Add(panelCabecalho);

        var btn = CriarButton
        (new Point(lblMedico.Right + 10, lblMedico.Top),
            new Size(25, 25)
        );
        btn.Text = "➕";
        panelCabecalho.Controls.Add(btn);

        // recebe o nome do médico e retorna um TextBox pronto
        TextBox criarTxtMedico(string m)
        {
            var txt = CriarTextBox(Point.Empty, new Size(panelCabecalho.Width - 60, 20), text: m);
            txt.Font = fonte;
            txt.CharacterCasing = CharacterCasing.Upper;
            return txt;
        }
        Button criarBtnRemove(FlowLayoutPanel layoutPanel, Panel panel, Point point)
        {
            var btnRemove = CriarButton(point, new Size(25, 25));
            btnRemove.Text = "➖";
            btnRemove.Click += (s, e) =>
            {
                if (s is not Button btn)
                    return;
                btn.Dispose();
                panel.Dispose();
                layoutPanel.Controls.Remove(panel);
            };
            return btnRemove;
        }
        btn.Click += (s, e) =>
        {
            var panel = CriarPanel(Point.Empty, new Size(panelCabecalho.Width, 25), panelMedicos.BackColor);
            var txt = criarTxtMedico("");
            var btn = criarBtnRemove(panelMedicos, panel, new Point(txt.Right + 5, txt.Top));
            panel.Controls.AddRange(txt, btn);
            panelMedicos.Controls.Add(panel);
        };
        foreach (var medico in valores.Medicos ?? [])
        {
            Debug.WriteLine(medico.ToString());
            var panel = CriarPanel(Point.Empty, new Size(panelCabecalho.Width, 25), panelMedicos.BackColor);
            var txt = criarTxtMedico(medico);
            var btnRemover = criarBtnRemove(panelMedicos, panel, new Point(txt.Right + 5, txt.Top));
            panel.Controls.AddRange(txt, btnRemover);
            panelMedicos.Controls.Add(panel);
        }
        return
        [
            lblCaminho, txtCaminho,
            lblEmail, txtEmail,
            lblSenha, txtSenha,
            lblEstabelecimento, txtEstabelecimento,
            lblCid, txtCid,
            lblPrioridade, txtPrioridade,
            lblUrgencia, txtUrgencia,
            panelMedicos,
        ];
    }
    private static (Label, TextBox) CriarCampo(ref Point posicao, string titulo, string valor)
    {
        var lbl = CriarLabel(posicao,
            fonte,
            titulo,
            Color.Black);
        lbl.Height = 12;

        var txt = CriarTextBox(
            new Point(posicao.X, lbl.Bottom + 5),
            new Size(300, 30),
            titulo);

        txt.Text = valor;
        txt.Font = fonte;

        posicao.Y = txt.Bottom + 5;

        return (lbl, txt);
    }
    public static ListView CriarListView(Dictionary<string, string> items, int width, int height, int limite = 20)
    {
        var lvw = new ListView
        {
            Height = height,
            Width = width,
            BorderStyle = BorderStyle.FixedSingle,
            FullRowSelect = true,
            GridLines = true,
            View = View.Details
        };
        int sizeId = 100;
        int sizeArquivos = width - sizeId;

        lvw.Columns.Add("Id", sizeId);
        lvw.Columns.Add("Arquivos", sizeArquivos);
        //lvw.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

        var lista = items.ToList();
        for (int i = 0; i < Math.Min(items.Count, limite); i++)
        {
            var item = lista[i];
            ListViewItem linha = new(item.Key);
            var caminhoArquivo = item.Value;
            linha.Tag = caminhoArquivo;
            var arquirvoSemExtensao = Path.GetFileNameWithoutExtension(caminhoArquivo);
            //MessageBox.Show(arquirvoSemExtensao);
            var dadosArquivo = FormatarNomeArquivo(arquirvoSemExtensao);
            linha.SubItems.Add(dadosArquivo);
            lvw.Items.Add(linha);
        }
        return lvw;
    }
    private static string FormatarNomeArquivo(string nomeArquivo)
    {
        var ano = nomeArquivo.Substring(12, 4);
        var mes = nomeArquivo.Substring(16, 2);
        var dia = nomeArquivo.Substring(18, 2);
        var hora = nomeArquivo.Substring(20, 2);
        var minuto = nomeArquivo.Substring(22, 2);
        return $"Atendimento de {dia}/{mes}/{ano} às {hora} hora(s) e {minuto} minuto(s)";
    }
}
