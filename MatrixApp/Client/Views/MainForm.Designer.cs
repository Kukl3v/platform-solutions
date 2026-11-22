#nullable enable

using Client.Services;

namespace Client.Views;

partial class MainForm
{
    private System.ComponentModel.IContainer? components;
    private BindingSource viewModelSource = null!;
    private TextBox rowCountTextBox = null!;
    private TextBox columnCountTextBox = null!;
    private TextBox minimumTextBox = null!;
    private TextBox maximumTextBox = null!;
    private Label rowCountErrorLabel = null!;
    private Label columnCountErrorLabel = null!;
    private Label minimumErrorLabel = null!;
    private Label maximumErrorLabel = null!;
    private CheckBox useFractionsCheckBox = null!;
    private Button generateButton = null!;
    private Button sortButton = null!;
    private Label statusLabel = null!;
    private MatrixGridVirtual matrixGrid = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            viewModel.MatrixPresentationChanged -= OnMatrixPresentationChanged;
            viewModel.Dispose();
            components?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent(MatrixStorageService storage)
    {
        components = new System.ComponentModel.Container();
        viewModelSource = new BindingSource(components);
        rowCountTextBox = new TextBox();
        columnCountTextBox = new TextBox();
        minimumTextBox = new TextBox();
        maximumTextBox = new TextBox();
        rowCountErrorLabel = CreateErrorLabel();
        columnCountErrorLabel = CreateErrorLabel();
        minimumErrorLabel = CreateErrorLabel();
        maximumErrorLabel = CreateErrorLabel();
        useFractionsCheckBox = new CheckBox();
        generateButton = new Button();
        sortButton = new Button();
        statusLabel = new Label();
        matrixGrid = new MatrixGridVirtual(storage);

        var rootLayout = new TableLayoutPanel();
        var fieldsLayout = new TableLayoutPanel();
        var buttonsLayout = new TableLayoutPanel();
        var statusLayout = new TableLayoutPanel();

        SuspendLayout();

        rootLayout.ColumnCount = 1;
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayout.Dock = DockStyle.Fill;
        rootLayout.Padding = new Padding(16);
        rootLayout.RowCount = 5;
        rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        fieldsLayout.AutoSize = true;
        fieldsLayout.ColumnCount = 2;
        fieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        fieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        fieldsLayout.Dock = DockStyle.Top;
        fieldsLayout.RowCount = 2;
        fieldsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        fieldsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        fieldsLayout.Controls.Add(CreateFieldPanel("Строки", rowCountTextBox, rowCountErrorLabel), 0, 0);
        fieldsLayout.Controls.Add(CreateFieldPanel("Столбцы", columnCountTextBox, columnCountErrorLabel), 1, 0);
        fieldsLayout.Controls.Add(CreateFieldPanel("Минимум", minimumTextBox, minimumErrorLabel), 0, 1);
        fieldsLayout.Controls.Add(CreateFieldPanel("Максимум", maximumTextBox, maximumErrorLabel), 1, 1);

        useFractionsCheckBox.AutoSize = true;
        useFractionsCheckBox.Margin = new Padding(4, 6, 4, 12);
        useFractionsCheckBox.Text = "Использовать дробные числа";

        buttonsLayout.AutoSize = true;
        buttonsLayout.ColumnCount = 2;
        buttonsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        buttonsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        buttonsLayout.Dock = DockStyle.Top;
        buttonsLayout.RowCount = 1;
        buttonsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        generateButton.Dock = DockStyle.Fill;
        generateButton.Margin = new Padding(4, 0, 6, 8);
        generateButton.UseVisualStyleBackColor = true;
        sortButton.Dock = DockStyle.Fill;
        sortButton.Margin = new Padding(6, 0, 4, 8);
        sortButton.UseVisualStyleBackColor = true;
        buttonsLayout.Controls.Add(generateButton, 0, 0);
        buttonsLayout.Controls.Add(sortButton, 1, 0);

        statusLayout.AutoSize = true;
        statusLayout.ColumnCount = 1;
        statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        statusLayout.Dock = DockStyle.Top;
        statusLayout.Margin = new Padding(4, 2, 4, 8);
        statusLayout.RowCount = 1;
        statusLabel.AutoEllipsis = true;
        statusLabel.AutoSize = true;
        statusLabel.Dock = DockStyle.Fill;
        statusLabel.Padding = new Padding(0, 3, 0, 0);
        statusLayout.Controls.Add(statusLabel, 0, 0);

        matrixGrid.BorderStyle = BorderStyle.FixedSingle;
        matrixGrid.Dock = DockStyle.Fill;
        matrixGrid.Margin = new Padding(4, 4, 4, 0);

        rootLayout.Controls.Add(fieldsLayout, 0, 0);
        rootLayout.Controls.Add(useFractionsCheckBox, 0, 1);
        rootLayout.Controls.Add(buttonsLayout, 0, 2);
        rootLayout.Controls.Add(statusLayout, 0, 3);
        rootLayout.Controls.Add(matrixGrid, 0, 4);

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(820, 620);
        Controls.Add(rootLayout);
        MinimumSize = new Size(620, 480);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Большие матрицы";

        ResumeLayout(false);
    }

    private static TableLayoutPanel CreateFieldPanel(string caption, TextBox textBox, Label errorLabel)
    {
        var panel = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 1,
            Dock = DockStyle.Top,
            Margin = new Padding(4, 0, 8, 4),
            RowCount = 3
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var label = new Label
        {
            AutoSize = true,
            Font = new Font(SystemFonts.MessageBoxFont ?? SystemFonts.DefaultFont, FontStyle.Bold),
            Text = caption
        };
        textBox.Dock = DockStyle.Top;
        textBox.Margin = new Padding(0, 3, 0, 0);

        panel.Controls.Add(label, 0, 0);
        panel.Controls.Add(textBox, 0, 1);
        panel.Controls.Add(errorLabel, 0, 2);
        return panel;
    }

    private static Label CreateErrorLabel() => new()
    {
        AutoEllipsis = true,
        AutoSize = true,
        ForeColor = Color.Firebrick,
        Margin = new Padding(0, 2, 0, 0),
        MinimumSize = new Size(0, 17)
    };
}
