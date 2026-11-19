namespace listar_exames.helpers;

partial class MensagemUI
{
    public Label? lblMensagem;
    public Panel? pnlMensagem;
    public MensagemUI() { }
    public MensagemUI(Panel panelNotificacao, Label labelNotificacao)
    {
        pnlMensagem = panelNotificacao;
        lblMensagem = labelNotificacao;
    }
    public async Task LimparMensagem(int? delay = null)
    {
        await Task.Delay(delay ?? 10000);
        if (lblMensagem is not null)
            lblMensagem.Text = "";
        if (pnlMensagem is not null)
            pnlMensagem.Visible = false;
    }
    public void MostrarErro(string texto, int? fontSize = null)
    {
        if (pnlMensagem is null || lblMensagem is null) return;
        pnlMensagem.BackColor = Color.MistyRose;
        lblMensagem.ForeColor = Color.DarkRed;
        lblMensagem.Text = "⚠️ " + texto;
        pnlMensagem.Visible = true;
        if (fontSize.HasValue)
        {
            lblMensagem.Font = new Font("Arial", fontSize.Value);
        }
    }
    public void MostrarSucesso(string texto)
    {
        if (pnlMensagem is null || lblMensagem is null) return;
        pnlMensagem.BackColor = Color.Honeydew;
        lblMensagem.ForeColor = Color.DarkGreen;
        lblMensagem.Text = "✔️ " + texto;
        pnlMensagem.Visible = true;
    }
    public void MostrarInfo(string texto)
    {
        if (pnlMensagem is null || lblMensagem is null) return;
        pnlMensagem.BackColor = Color.LightCyan;
        lblMensagem.ForeColor = Color.DarkBlue;
        lblMensagem.Text = "💡 " + texto;
        pnlMensagem.Visible = true;
    }

}
