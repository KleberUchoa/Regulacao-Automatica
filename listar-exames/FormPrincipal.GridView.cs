using listar_exames.helpers;
using regulacao.core.models;
using System.Drawing.Drawing2D;
namespace listar_exames;

public partial class FormPrincipal : Form
{
    private enum HoverAcao
    {
        Nenhuma,
        Editar,
        Excluir
    }

    private HoverAcao hoverAtual = HoverAcao.Nenhuma;
    public DataGridView CriarDataGridView(Point location, Size size)
    {
        var dtgv = new DataGridView()
        {
            Location = location,
            Size = size,
            Font = new Font("Segoe UI", 7),
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToResizeColumns = false,
            AllowUserToResizeRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        };
        //retira a cor de seleção
        dtgv.DefaultCellStyle.SelectionBackColor = dtgv.DefaultCellStyle.BackColor;
        dtgv.DefaultCellStyle.SelectionForeColor = dtgv.DefaultCellStyle.ForeColor;

        dtgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", DataPropertyName = "Id", Width = 20 });
        dtgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nome", HeaderText = "Nome", DataPropertyName = "Nome", Width = 120 });
        dtgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cpf", HeaderText = "CPF", DataPropertyName = "Cpf", Width = 70 });
        dtgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cns", HeaderText = "CNS", DataPropertyName = "Cns", Width = 70 });
        dtgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Data", HeaderText = "Data", DataPropertyName = "DataAtendimento", Width = 70 });
        dtgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Historico", HeaderText = "Histórico", DataPropertyName = "Historico", Width = 100 });

        var colunaProcedimentos = new DataGridViewComboBoxColumn
        {
            Name = "Procedimentos",
            HeaderText = "Procedimentos",
            Width = 85,
            FlatStyle = FlatStyle.Flat,
            DisplayMember = "Nome",
            ValueMember = "Codigo"
        };
        dtgv.Columns.Add(colunaProcedimentos);

        var colunaAcoes = new DataGridViewTextBoxColumn
        {
            Name = "Acoes",
            HeaderText = "Ações",
            Width = 90,
            ReadOnly = true,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        dtgv.Columns.Add(colunaAcoes);
        dtgv.CellPainting += Dtgv_CellPainting;
        dtgv.CellMouseClick += Dtgv_CellMouseClick;
        dtgv.MouseMove += Dtgv_MouseMove;
        dtgv.MouseLeave += dtgv_MouseLeave;
        dtgv.CurrentCellDirtyStateChanged += Dtgv_CurrentCellDirtyStateChanged;
        dtgv.DataError += Dtgv_DataError;
        return dtgv;
    }
    private static (Rectangle editar, Rectangle excluir) GetBotoesRects(Rectangle cellBounds)
    {
        int margem = 3;
        int espaco = 4;
        int largura = (cellBounds.Width - margem * 2 - espaco) / 2;
        int altura = cellBounds.Height - margem * 2;

        var editar = new Rectangle(cellBounds.Left + margem / 2, cellBounds.Top + margem / 2, largura, altura);
        var excluir = new Rectangle(editar.Right + espaco, cellBounds.Top + margem / 2, largura, altura);
        return (editar, excluir);
    }
    private void Dtgv_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        var g = e.Graphics;
        if (e.RowIndex < 0 || listAtendimentos?.Columns[e.ColumnIndex].Name != "Acoes")
            return;
        //Limpa fundo ante de desenhar por cima
        e.PaintBackground(e.ClipBounds, true);

        var (rectEditar, rectExcluir) = GetBotoesRects(e.CellBounds);
        if (g is null)
            return;
        DesenharBotao(rectEditar, g, AppResources.Edit, Color.DodgerBlue);
        DesenharBotao(rectExcluir, g, AppResources.Cross, Color.DodgerBlue);
        //Avisa o fim do processo de pintura da célula
        e.Handled = true;
    }
    //desenha retângulo com cantos arredondados
    private void DesenharBotao(Rectangle bounds, Graphics graphics, Image image, Color color)
    {
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using var path = RoundedRect(bounds, 6);
        using var brush = new SolidBrush(Color.FromArgb(40, color));
        using var pen = new Pen(Color.FromArgb(120, color));

        graphics.FillPath(brush, path);
        graphics.DrawPath(pen, path);
        //Reduz o tamanho da imagem para caber no botão
        int iconSize = Math.Min(bounds.Width, bounds.Height) - 8;

        var rectIcon = new Rectangle(
            bounds.X + (bounds.Width - iconSize) / 2,
            bounds.Y + (bounds.Height - iconSize) / 2,
            iconSize,
            iconSize
        );
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.DrawImage(
            image,
            rectIcon
         );

    }
    private static HoverAcao GetHoverAcao(Point mouse, Rectangle rectangle)
    {
        var (editar, excluir) = GetBotoesRects(rectangle);
        if (editar.Contains(mouse))
            return HoverAcao.Editar;
        if (excluir.Contains(mouse))
            return HoverAcao.Excluir;
        return HoverAcao.Nenhuma;
    }
    private void Dtgv_CellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (sender is not DataGridView dtgv)
            return;
        if (e.RowIndex < 0 || listAtendimentos?.Columns[e.ColumnIndex].Name != "Acoes")
            return;
        var cellSize = listAtendimentos.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false).Size;
        var cellLocal = new Rectangle(Point.Empty, cellSize);

        HoverAcao acao = GetHoverAcao(e.Location, cellLocal);

        var linha = dtgv.Rows[e.RowIndex].DataBoundItem; //pega item da linha selecionada
        var atendimento = linha as Atendimento;

        switch (acao)
        {
            case HoverAcao.Editar:
                editMode = true;
                ReconstruirFormularioAtendimento(atendimento);
                break;
            case HoverAcao.Excluir:

                if (Components.CriarDialogBoxConfimacao("Excluir",
                    $"Excluir atendimento?")
                    == DialogResult.OK)
                {
                    atendimentos.Remove(atendimento ?? throw new InvalidOperationException("Atendimento não encontrado"));
                }
                break;
        }
    }
    private void dtgv_MouseLeave(object? sender, EventArgs e)
    {
        if (sender is not DataGridView dtgv)
            return;
        hoverAtual = HoverAcao.Nenhuma;
        dtgv.Cursor = Cursors.Default;
        tltAtendimento.Hide(dtgv);
    }
    private void Dtgv_MouseMove(object? sender, MouseEventArgs e)
    {
        if (sender is not DataGridView dtgv)
            return;
        var hit = listAtendimentos?.HitTest(e.X, e.Y);

        if (hit?.RowIndex < 0 ||
            hit?.ColumnIndex < 0 ||
            listAtendimentos?.Columns[hit!.ColumnIndex].Name != "Acoes")
        {
            hoverAtual = HoverAcao.Nenhuma;
            dtgv.Cursor = Cursors.Default;
            tltAtendimento.Hide(dtgv);
            dtgv.ShowCellToolTips = true;
            return;
        }
        dtgv.ShowCellToolTips = false; //evita conflito com o tooltip padrão do datagridview
        Rectangle cell =
            dtgv.GetCellDisplayRectangle(
                hit.ColumnIndex,
                hit.RowIndex,
                false);

        HoverAcao acao = GetHoverAcao(e.Location, cell);
        dtgv.Cursor = acao == HoverAcao.Nenhuma ? Cursors.Default : Cursors.Hand;

        if (acao != hoverAtual)
        {
            hoverAtual = acao;
            tltAtendimento.Hide(dtgv);
            string? texto = acao switch
            {
                HoverAcao.Editar => "Editar Atendimento",
                HoverAcao.Excluir => "Excluir Atendimento",
                _ => null
            };
            if (texto != null)
                tltAtendimento.Show(texto, dtgv, e.X + 18, e.Y + 18);
        }
    }
    private void AtualizarProcedimentoLinha(DataGridViewRow row)
    {
        if (row.DataBoundItem is not Atendimento atendimento)
            return;

        if (row.Cells["Procedimentos"] is not DataGridViewComboBoxCell cell)
            return;

        var procedimentos = atendimento.Procedimentos ?? [];

        cell.DataSource = null;
        cell.DataSource = procedimentos;
        cell.DisplayMember = "Nome";
        cell.ValueMember = "Codigo";

        if (procedimentos.Count > 0)
            cell.Value = procedimentos[0].Codigo;
    }
    private void AtualizarProcedimentosGrid(int? index = null)
    {
        if (index.HasValue)
        {
            if (index.Value >= 0 && index.Value < listAtendimentos?.Rows.Count)
                AtualizarProcedimentoLinha(listAtendimentos.Rows[index.Value]);
            return;
        }
        foreach (DataGridViewRow row in listAtendimentos!.Rows)
            AtualizarProcedimentoLinha(row);
    }
    private void Dtgv_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
    {
        if (sender is not DataGridView dtgv)
            return;
        if (dtgv.CurrentCell is DataGridViewComboBoxCell && dtgv.IsCurrentCellDirty)
            dtgv.CommitEdit(DataGridViewDataErrorContexts.Commit);
    }
    private void Dtgv_DataError(object? sender, DataGridViewDataErrorEventArgs e)
    {
        // evita que o grid quebre se o Value de alguma célula não bater com o DataSource
        e.ThrowException = false;
    }
    private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        int diameter = radius * 2;
        //O diâmetro obedece os limite do retângulo repassado em bounds
        if (diameter > bounds.Width)
            diameter = bounds.Width;
        if (diameter > bounds.Height)
            diameter = bounds.Height;
        path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        return path;
    }
}