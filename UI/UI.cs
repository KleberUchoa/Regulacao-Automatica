using System.Drawing.Drawing2D;
namespace UI;

public class UI : Form
{
    protected void Arredondar()
    {
        GraphicsPath path = new GraphicsPath();
        int r = 20;
        path.AddArc(0, 0, r, r, 180, 90);
        path.AddArc(Width - r, 0, r, r, 270, 90);
        path.AddArc(Width - r, Height - r, r, r, 0, 90);
        path.AddArc(0, Height - r, r, r, 90, 90);
        path.CloseFigure();

        Region = new Region(path);
    }
    protected async Task FadeInAsync()
    {
        while (Opacity < 1)
        {
            Opacity = Math.Min(1, Opacity + 0.05);
            await Task.Delay(15);
        }
    }
    protected async Task FecharAsync()
    {
        while (Opacity > 0)
        {
            Opacity -= 0.05;
            await Task.Delay(15);
        }
        Close();
    }
    protected void ButtonPaint(object? sender, PaintEventArgs e)
    {
        var btn = sender as Button;

        if (btn is null)
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
}
