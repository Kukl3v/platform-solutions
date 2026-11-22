using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Client.Models;
using Client.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Client.ViewModels;

public sealed partial class MainViewModel : ObservableValidator, IDisposable
{
    private readonly MatrixStorageService storage;
    private readonly MatrixColumnSorter sorter;
    private readonly CancellationTokenSource lifetimeCancellation = new();
    private MatrixDescriptor? matrix;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Укажите количество строк.")]
    [WholeNumberRangeText(10, 20_000)]
    private string rowCountText = "1000";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Укажите количество столбцов.")]
    [WholeNumberRangeText(10, 20_000)]
    private string columnCountText = "1000";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Укажите минимум.")]
    [MatrixNumberText]
    private string minimumText = "0";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Укажите максимум.")]
    [MatrixNumberText]
    [MaximumNotLessThan(nameof(MinimumText))]
    private string maximumText = "100";

    [ObservableProperty]
    private bool useFractions;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string currentOperation = string.Empty;

    [ObservableProperty]
    private string statusMessage = "Задайте параметры и создайте матрицу.";

    public MainViewModel(MatrixStorageService storage, MatrixColumnSorter sorter)
    {
        this.storage = storage;
        this.sorter = sorter;
        ErrorsChanged += OnErrorsChanged;
        ValidateAllProperties();
    }

    public event EventHandler<MatrixPresentationChangedEventArgs>? MatrixPresentationChanged;

    public string GenerateButtonText => IsBusy && CurrentOperation == "Генерация"
        ? "Подождите"
        : "Сгенерировать";

    public string SortButtonText => IsBusy && CurrentOperation == "Сортировка"
        ? "Подождите"
        : "Упорядочить столбцы";

    public string RowCountError => GetFirstError(nameof(RowCountText));
    public string ColumnCountError => GetFirstError(nameof(ColumnCountText));
    public string MinimumError => GetFirstError(nameof(MinimumText));
    public string MaximumError => GetFirstError(nameof(MaximumText));

    [RelayCommand(CanExecute = nameof(CanGenerate))]
    private async Task GenerateAsync()
    {
        ValidateAllProperties();
        if (HasErrors)
        {
            StatusMessage = "Исправьте ошибки в параметрах.";
            return;
        }

        var options = CreateOptions();
        await RunOperationAsync("Генерация", async cancellationToken =>
        {
            var descriptor = await Task.Run(
                () => storage.Generate(options, cancellationToken),
                cancellationToken);
            matrix = descriptor;
            MatrixPresentationChanged?.Invoke(
                this,
                new MatrixPresentationChangedEventArgs(descriptor, columnOrder: null));
            StatusMessage = $"Матрица {descriptor.Rows:N0} × {descriptor.Columns:N0} создана.";
        });
    }

    [RelayCommand(CanExecute = nameof(CanSortColumns))]
    private async Task SortColumnsAsync()
    {
        var descriptor = matrix;
        if (descriptor is null)
        {
            return;
        }

        await RunOperationAsync("Сортировка", async cancellationToken =>
        {
            var order = await Task.Run(
                () => sorter.Sort(descriptor, cancellationToken),
                cancellationToken);
            MatrixPresentationChanged?.Invoke(
                this,
                new MatrixPresentationChangedEventArgs(descriptor, order));
            StatusMessage = "Столбцы упорядочены по возрастанию минимальных элементов.";
        });
    }

    private bool CanGenerate() => !IsBusy && !HasErrors;
    private bool CanSortColumns() => !IsBusy && matrix is not null;

    private async Task RunOperationAsync(
        string operation,
        Func<CancellationToken, Task> action)
    {
        IsBusy = true;
        CurrentOperation = operation;
        StatusMessage = $"{operation}…";

        try
        {
            await action(lifetimeCancellation.Token);
        }
        catch (OperationCanceledException) when (lifetimeCancellation.IsCancellationRequested)
        {
            StatusMessage = "Операция отменена.";
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or
                                              InvalidDataException or ArgumentException or OverflowException)
        {
            StatusMessage = $"Ошибка: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
            CurrentOperation = string.Empty;
        }
    }

    private MatrixGenerationOptions CreateOptions()
    {
        var rows = int.Parse(RowCountText, CultureInfo.InvariantCulture);
        var columns = int.Parse(ColumnCountText, CultureInfo.InvariantCulture);

        if (UseFractions)
        {
            return new MatrixGenerationOptions(
                rows,
                columns,
                MatrixNumberKind.Fractional,
                0,
                0,
                double.Parse(MinimumText, CultureInfo.InvariantCulture),
                double.Parse(MaximumText, CultureInfo.InvariantCulture));
        }

        return new MatrixGenerationOptions(
            rows,
            columns,
            MatrixNumberKind.Integer,
            long.Parse(MinimumText, CultureInfo.InvariantCulture),
            long.Parse(MaximumText, CultureInfo.InvariantCulture),
            0,
            0);
    }

    private string GetFirstError(string propertyName) =>
        GetErrors(propertyName).FirstOrDefault()?.ErrorMessage ?? string.Empty;

    private void OnErrorsChanged(object? sender, DataErrorsChangedEventArgs eventArgs)
    {
        var errorProperty = eventArgs.PropertyName switch
        {
            nameof(RowCountText) => nameof(RowCountError),
            nameof(ColumnCountText) => nameof(ColumnCountError),
            nameof(MinimumText) => nameof(MinimumError),
            nameof(MaximumText) => nameof(MaximumError),
            _ => null
        };

        if (errorProperty is not null)
        {
            OnPropertyChanged(errorProperty);
        }

        GenerateCommand.NotifyCanExecuteChanged();
    }

    partial void OnUseFractionsChanged(bool value)
    {
        ValidateProperty(MinimumText, nameof(MinimumText));
        ValidateProperty(MaximumText, nameof(MaximumText));
    }

    partial void OnMinimumTextChanged(string value)
    {
        ValidateProperty(MaximumText, nameof(MaximumText));
    }

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(GenerateButtonText));
        OnPropertyChanged(nameof(SortButtonText));
        GenerateCommand.NotifyCanExecuteChanged();
        SortColumnsCommand.NotifyCanExecuteChanged();
    }

    partial void OnCurrentOperationChanged(string value)
    {
        OnPropertyChanged(nameof(GenerateButtonText));
        OnPropertyChanged(nameof(SortButtonText));
    }

    public void Dispose()
    {
        ErrorsChanged -= OnErrorsChanged;
        lifetimeCancellation.Cancel();
    }
}

public sealed class MatrixPresentationChangedEventArgs(
    MatrixDescriptor descriptor,
    int[]? columnOrder) : EventArgs
{
    public MatrixDescriptor Descriptor { get; } = descriptor;
    public int[]? ColumnOrder { get; } = columnOrder;
}
