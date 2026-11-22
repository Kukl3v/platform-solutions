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

        progressBar.DataBindings.Add(
            nameof(ProgressBar.Value),
            viewModelSource,
            nameof(MainViewModel.ProgressPercentage),
            false,
            DataSourceUpdateMode.Never);

        generateButton.Command = viewModel.GenerateCommand;
        sortButton.Command = viewModel.SortColumnsCommand;
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
}

