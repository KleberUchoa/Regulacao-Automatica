namespace UI;

public class DialogUI : UI
{
    public string Titulo { get; private set; }
    public int Largura { get; private set; }
    public int Altura { get; private set; }
    private Panel? corpo;
    public Panel Corpo => corpo ?? throw new InvalidOperationException("O corpo do diálogo não foi inicializado.");
    public Dictionary<string, string> ListaArquivos { get; private set; } = new();
    public string? ArquivoSelecionado { get; private set; }
    private readonly ToolTip toolTip = new();
    private readonly Image imgSalvar;
    private readonly Image imgCancelar;
    private readonly Image imgFechar;

    public DialogUI(string titulo, int largura, int altura, Image imgSalvar, Image imgCancelar, Image imgFechar, Color? backColor = null)
    {
        this.imgSalvar = imgSalvar;
        this.imgCancelar = imgCancelar;
        this.imgFechar = imgFechar;
        Titulo = titulo;
        Largura = largura;
        Altura = altura;
        CriarComponentes((backColor == null) ? Color.White : backColor.Value);
    }

    private void CriarComponentes(Color backColor)
    {
        ClientSize = new Size(Largura, Altura);
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        BackColor = backColor;
        Opacity = 0;
        Load += Dialog_Load;

        const int margem = 10;
        const int alturaCabecalho = 40;
        const int alturaRodape = 45;
        const int btnWidth = 20;
        int controlWidth = ClientSize.Width - 2 * margem;

        // Cabeçalho
        var pnlCabecalho = new Panel
        {
            Location = new Point(margem, margem),
            Height = alturaCabecalho,
            Width = controlWidth
        };
        var lblTitulo = new Label
        {
            Location = new Point(0, 0),
            Size = new Size(controlWidth - btnWidth - margem, alturaCabecalho),
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
            Text = Titulo,
            TextAlign = ContentAlignment.MiddleCenter
        };

        var btnFechar = CriarBotaoIcone(imgFechar, btnWidth);
        btnFechar.Location = new Point(controlWidth - btnFechar.Width - margem, 0);
        toolTip.SetToolTip(btnFechar, "Fechar");
        btnFechar.Click += CancelResultado;

        pnlCabecalho.Controls.Add(lblTitulo);
        pnlCabecalho.Controls.Add(btnFechar);

        int corpoY = pnlCabecalho.Bottom + margem;
        int alturaCorpo = ClientSize.Height - alturaCabecalho - alturaRodape - 4 * margem;
        // Corpo
        var pnlCorpo = new Panel
        {
            Location = new Point(margem, corpoY),
            Padding = new Padding(margem),
            Width = controlWidth,
            Height = alturaCorpo
        };
        corpo = pnlCorpo;
        // Rodapé
        var pnlRodape = new Panel
        {
            Location = new Point(margem, pnlCorpo.Bottom + margem),
            Height = alturaRodape,
            Width = controlWidth
        };

        var botoes = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Width = controlWidth,
            Height = pnlRodape.Height,
            Location = new Point(0, 0),
            Padding = new Padding(10),
        };
        var btnOk = CriarBotaoIcone(imgSalvar, 30);
        toolTip.SetToolTip(btnOk, "Confirmar");
        var btnCancel = CriarBotaoIcone(imgCancelar, 30);
        toolTip.SetToolTip(btnCancel, "Cancelar");
        btnOk.Margin = new Padding(40, 0, 0, 0);
        btnCancel.Margin = new Padding(40, 0, 0, 0);
        btnOk.Click += OkResultado;
        btnCancel.Click += CancelResultado;
        botoes.Controls.Add(btnOk); // OK mais à direita
        botoes.Controls.Add(btnCancel);
        pnlRodape.Controls.Add(botoes);

        // Ordem de adição: docks fixos primeiro, Fill por último
        Controls.Add(pnlCabecalho);
        Controls.Add(pnlRodape);
        Controls.Add(pnlCorpo);
    }

    private Button CriarBotaoIcone(Image icone, int tamanho)
    {
        var btn = new Button
        {
            Size = new Size(tamanho, tamanho),
            Image = icone,
            FlatStyle = FlatStyle.Flat,
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, Color.DodgerBlue);
        btn.MouseEnter += (_, _) =>
        {
            btn.BackColor = Color.FromArgb(40, Color.DodgerBlue);
            btn.Invalidate();
        };
        btn.MouseLeave += (_, _) =>
        {
            btn.BackColor = btn.Parent?.BackColor ?? Color.Transparent;
            btn.Invalidate();
        };
        btn.Paint += ButtonPaint;
        return btn;
    }

    private async void Dialog_Load(object? sender, EventArgs e)
    {
        Rectangle area = Screen.PrimaryScreen?.WorkingArea ?? new();
        Location = new Point((area.Width - Width) / 2, (area.Height - Height) / 2);
        Arredondar();
        await FadeInAsync();
    }

    private async void OkResultado(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        await FecharAsync();
    }

    private async void CancelResultado(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        await FecharAsync();
    }

    public void AdicionarControle(Control control)
    {
        Corpo.Controls.Add(control);
    }
    public void AdicionarControles(params Control[] controls)
    {
        Corpo.Controls.AddRange(controls);
    }
}
