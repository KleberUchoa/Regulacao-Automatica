namespace UI;

public enum AlertaType
{
    Success,
    Error,
    Warning,
    Info
}
public class AlertaUI : UI
{
    private Panel? panelColor;
    private Label? lblTitulo;
    private Label? lblMensagem;
    private Label? lblIcone;
    private Button? btnFechar;
    private System.Windows.Forms.Timer? tempoFechar;

    public AlertaUI(string titulo, string mensagem, AlertaType tipo)
    {
        CriarComponentes();
        var tituloLabel = lblTitulo ?? throw new InvalidOperationException("O título do alerta não foi inicializado.");
        var mensagemLabel = lblMensagem ?? throw new InvalidOperationException("A mensagem do alerta não foi inicializada.");
        var faixaCor = panelColor ?? throw new InvalidOperationException("A faixa do alerta não foi inicializada.");
        var iconeLabel = lblIcone ?? throw new InvalidOperationException("O ícone do alerta não foi inicializado.");
        tituloLabel.Text = titulo;
        mensagemLabel.Text = mensagem;
        switch (tipo)
        {
            case AlertaType.Success:
                faixaCor.BackColor = Color.SeaGreen;
                iconeLabel.Text = "✔";
                break;
            case AlertaType.Error:
                faixaCor.BackColor = Color.Firebrick;
                iconeLabel.Text = "✖";
                break;
            case AlertaType.Warning:
                faixaCor.BackColor = Color.DarkOrange;
                iconeLabel.Text = "⚠";
                break;
            case AlertaType.Info:
                faixaCor.BackColor = Color.DodgerBlue;
                iconeLabel.Text = "ℹ";
                break;
        }
    }
    private void CriarComponentes()
    {
        Width = 360;
        Height = 180;
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        StartPosition = FormStartPosition.Manual;
        BackColor = Color.White;
        Opacity = 0;

        panelColor = new Panel
        {
            Dock = DockStyle.Left,
            Width = 8
        };
        lblIcone = new Label
        {
            Font = new Font("Segoe UI", 22, FontStyle.Bold),
            Size = new Size(40, 40),
            Location = new Point(20, 20)
        };
        lblTitulo = new Label
        {
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(70, 15)
        };
        lblMensagem = new Label
        {
            Font = new Font("Segoe UI", 9),
            AutoSize = true,
            Location = new Point(70, 40),
            MaximumSize = new Size(250, 0),
            AutoEllipsis = true
        };
        btnFechar = new Button
        {
            Text = "✖",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            Cursor = Cursors.Hand,
            Size = new Size(35, 35),
            Location = new Point(320, 5),
        };
        btnFechar.FlatAppearance.BorderSize = 0;
        btnFechar.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, Color.DodgerBlue);
        btnFechar.MouseEnter += (_, _) => btnFechar.BackColor = Color.FromArgb(40, Color.DodgerBlue);
        btnFechar.MouseLeave += (_, _) => btnFechar.BackColor = Color.White;
        btnFechar.Click += btnFechar_Click;

        Controls.Add(panelColor);
        Controls.Add(lblIcone);
        Controls.Add(lblTitulo);
        Controls.Add(lblMensagem);
        Controls.Add(btnFechar);
        tempoFechar = new System.Windows.Forms.Timer
        {
            Interval = 3000,
        };
        tempoFechar.Tick += tempoFechar_Tick;

        Load += AlertaUI_Load;
    }
    private async void btnFechar_Click(object? sender, EventArgs e)
    {
        await FecharAsync();
    }
    private async void tempoFechar_Tick(object? sender, EventArgs e)
    {
        tempoFechar?.Stop();
        await FecharAsync();
    }
    private async void AlertaUI_Load(object? sender, EventArgs e)
    {
        Rectangle area = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle();
        Location = new Point(area.Right - Width - 20, area.Bottom - Height - 20);
        Arredondar();
        await FadeInAsync();
        tempoFechar?.Start();
    }




}
public static class Alerta
{
    public static void Show(string titulo, string mensagem, AlertaType alertaTipo)
    {
        AlertaUI alertaUtil = new AlertaUI(titulo, mensagem, alertaTipo);
        alertaUtil.Show();
    }
}
