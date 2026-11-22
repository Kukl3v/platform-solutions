using Client.Services;
using Client.ViewModels;

namespace Client.Views;

public partial class MainForm : Form
{
    private readonly MainViewModel viewModel;

    public MainForm(MainViewModel viewModel, MatrixStorageService storage)
    {
        this.viewModel = viewModel;
        InitializeComponent(storage);
        ConfigureBindings();
        viewModel.MatrixPresentationChanged += OnMatrixPresentationChanged;
    }

    private void ConfigureBindings()
    {
        viewModelSource.DataSource = viewModel;

        BindText(rowCountTextBox, nameof(MainViewModel.RowCountText));
        BindText(columnCountTextBox, nameof(MainViewModel.ColumnCountText));
        BindText(minimumTextBox, nameof(MainViewModel.MinimumText));
        BindText(maximumTextBox, nameof(MainViewModel.MaximumText));

        useFractionsCheckBox.DataBindings.Add(
            nameof(CheckBox.Checked),
            viewModelSource,
            nameof(MainViewModel.UseFractions),
            false,
            DataSourceUpdateMode.OnPropertyChanged);

        BindLabel(rowCountErrorLabel, nameof(MainViewModel.RowCountError));
        BindLabel(columnCountErrorLabel, nameof(MainViewModel.ColumnCountError));
        BindLabel(minimumErrorLabel, nameof(MainViewModel.MinimumError));
        BindLabel(maximumErrorLabel, nameof(MainViewModel.MaximumError));
        BindLabel(statusLabel, nameof(MainViewModel.StatusMessage));
        BindLabel(generateButton, nameof(MainViewModel.GenerateButtonText));
        BindLabel(sortButton, nameof(MainViewModel.SortButtonText));
        BindLabel(compareButton, nameof(MainViewModel.CompareButtonText));
        BindText(firstMatrixPathTextBox, nameof(MainViewModel.FirstMatrixPath));
        BindText(secondMatrixPathTextBox, nameof(MainViewModel.SecondMatrixPath));
        BindLabel(comparisonStatusLabel, nameof(MainViewModel.ComparisonStatus));
        matchCountLabel.DataBindings.Add(
            nameof(Label.Text),
            viewModelSource,
            nameof(MainViewModel.MatchCount),
            true,
            DataSourceUpdateMode.Never,
            0,
            "Найдено совпадений: {0:N0}");
        saveButton.DataBindings.Add(
            nameof(Button.Enabled),
            viewModelSource,
            nameof(MainViewModel.CanSaveMatrix),
            false,
            DataSourceUpdateMode.Never);
        foreach (var control in new Control[]
                 {
                     firstMatrixPathTextBox,
                     secondMatrixPathTextBox,
                     firstBrowseButton,
                     secondBrowseButton
                 })
        {
            control.DataBindings.Add(
                nameof(Control.Enabled),
                viewModelSource,
                nameof(MainViewModel.CanSelectComparisonFiles),
                false,
                DataSourceUpdateMode.Never);
        }
        comparisonMatchesListBox.DataSource = viewModel.ComparisonMatches;

        generateButton.Command = viewModel.GenerateCommand;
        sortButton.Command = viewModel.SortColumnsCommand;
        compareButton.Command = viewModel.CompareCommand;
        cancelComparisonButton.Command = viewModel.CancelComparisonCommand;

        firstBrowseButton.Click += OnFirstBrowseClick;
        secondBrowseButton.Click += OnSecondBrowseClick;
        saveButton.Click += OnSaveClick;
    }

    private void BindText(TextBox textBox, string propertyName)
    {
        textBox.DataBindings.Add(
            nameof(TextBox.Text),
            viewModelSource,
            propertyName,
            false,
            DataSourceUpdateMode.OnPropertyChanged);
    }

    private void BindLabel(Control control, string propertyName)
    {
        control.DataBindings.Add(
            nameof(Control.Text),
            viewModelSource,
            propertyName,
            false,
            DataSourceUpdateMode.Never);
    }

    private void OnMatrixPresentationChanged(object? sender, MatrixPresentationChangedEventArgs eventArgs)
    {
        matrixGrid.Bind(eventArgs.Descriptor, eventArgs.ColumnOrder);
    }

    private void OnFirstBrowseClick(object? sender, EventArgs eventArgs)
    {
        var path = SelectMatrixFile(viewModel.FirstMatrixPath);
        if (path is not null)
        {
            viewModel.FirstMatrixPath = path;
        }
    }

    private void OnSecondBrowseClick(object? sender, EventArgs eventArgs)
    {
        var path = SelectMatrixFile(viewModel.SecondMatrixPath);
        if (path is not null)
        {
            viewModel.SecondMatrixPath = path;
        }
    }

    private void OnSaveClick(object? sender, EventArgs eventArgs)
    {
        using var dialog = new SaveFileDialog
        {
            AddExtension = true,
            DefaultExt = "txt",
            Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
            FileName = "matrix.txt",
            OverwritePrompt = true,
            Title = "Сохранить копию матрицы"
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            viewModel.SaveMatrixCopyCommand.Execute(dialog.FileName);
        }
    }

    private string? SelectMatrixFile(string currentPath)
    {
        using var dialog = new OpenFileDialog
        {
            CheckFileExists = true,
            Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
            Multiselect = false,
            Title = "Выберите файл матрицы"
        };

        if (!string.IsNullOrWhiteSpace(currentPath))
        {
            dialog.FileName = currentPath;
        }

        return dialog.ShowDialog(this) == DialogResult.OK ? dialog.FileName : null;
    }
}

