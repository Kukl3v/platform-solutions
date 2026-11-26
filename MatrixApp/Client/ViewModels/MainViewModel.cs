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
    private readonly MatrixComparisonService comparisonService;
    private readonly CancellationTokenSource lifetimeCancellation = new();
    private CancellationTokenSource? comparisonCancellation;
    private MatrixDescriptor? matrix;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Укажите количество строк.")]
    [WholeNumberRangeText(10, 500_000)]
    private string rowCountText = "1000";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Укажите количество столбцов.")]
    [WholeNumberRangeText(1, 20_000)]
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

    [ObservableProperty]
    private string firstMatrixPath = string.Empty;

    [ObservableProperty]
    private string secondMatrixPath = string.Empty;

    [ObservableProperty]
    private bool isComparing;

    [ObservableProperty]
    private long matchCount;

    [ObservableProperty]
    private string comparisonStatus = "Выберите два TXT-файла для сравнения.";

    public MainViewModel(
        MatrixStorageService storage,
        MatrixColumnSorter sorter,
        MatrixComparisonService comparisonService)
    {
        this.storage = storage;
        this.sorter = sorter;
        this.comparisonService = comparisonService;
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

    public string CompareButtonText => IsComparing ? "Сравниваю" : "Сравнить";
    public bool CanSaveMatrix => matrix is not null && !IsBusy && !IsComparing;
    public bool CanSelectComparisonFiles => !IsBusy && !IsComparing;
    public BindingList<string> ComparisonMatches { get; } = [];

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
            OnPropertyChanged(nameof(CanSaveMatrix));
            MatrixPresentationChanged?.Invoke(
                this,
                new MatrixPresentationChangedEventArgs(descriptor, columnOrder: null));
            StatusMessage = $"Матрица {descriptor.Rows:N0} × {descriptor.Columns:N0} создана.";
        });
    }

    [RelayCommand]
    private async Task SaveMatrixCopyAsync(string? destinationPath)
    {
        var descriptor = matrix;
        if (descriptor is null || string.IsNullOrWhiteSpace(destinationPath) || IsBusy || IsComparing)
        {
            return;
        }

        await RunOperationAsync("Сохранение", async cancellationToken =>
        {
            await storage.CopyMatrixAsync(descriptor, destinationPath, cancellationToken);
            StatusMessage = $"Копия матрицы сохранена: {destinationPath}";
        });
    }

    [RelayCommand(CanExecute = nameof(CanCompare))]
    private async Task CompareAsync()
    {
        var firstFilePath = FirstMatrixPath;
        var secondFilePath = SecondMatrixPath;
        ComparisonMatches.Clear();
        MatchCount = 0;
        IsComparing = true;
        ComparisonStatus = "Индексирую первую матрицу…";
        comparisonCancellation = CancellationTokenSource.CreateLinkedTokenSource(lifetimeCancellation.Token);
        var cancellationToken = comparisonCancellation.Token;
        var progress = new Progress<MatrixComparisonProgress>(ApplyComparisonProgress);

        try
        {
            var result = await Task.Run(
                () => comparisonService.Compare(
                    firstFilePath,
                    secondFilePath,
                    progress,
                    cancellationToken),
                cancellationToken);

            MatchCount = result.MatchCount;
            ComparisonStatus = result.ColumnCountsDiffer
                ? $"Сравнение завершено. Число столбцов различается ({result.FirstColumnCount} и {result.SecondColumnCount}); полные совпадения строк невозможны."
                : $"Сравнение завершено. Найдено совпадений: {result.MatchCount:N0}.";
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            ComparisonStatus = $"Сравнение отменено. Уже найдено совпадений: {MatchCount:N0}.";
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or
                                              InvalidDataException or ArgumentException or OverflowException)
        {
            ComparisonStatus = $"Ошибка: {exception.Message}";
        }
        finally
        {
            comparisonCancellation.Dispose();
            comparisonCancellation = null;
            IsComparing = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCancelComparison))]
    private void CancelComparison()
    {
        ComparisonStatus = "Отменяю сравнение…";
        comparisonCancellation?.Cancel();
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

    private bool CanGenerate() => !IsBusy && !IsComparing && !HasErrors;
    private bool CanSortColumns() => !IsBusy && !IsComparing && matrix is not null;
    private bool CanCompare() => !IsBusy && !IsComparing &&
                                 !string.IsNullOrWhiteSpace(FirstMatrixPath) &&
                                 !string.IsNullOrWhiteSpace(SecondMatrixPath);
    private bool CanCancelComparison() => IsComparing;

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

    private void ApplyComparisonProgress(MatrixComparisonProgress progress)
    {
        MatchCount = progress.TotalMatches;
        foreach (var match in progress.RecentMatches)
        {
            if (ComparisonMatches.Count == 1000)
            {
                ComparisonMatches.RemoveAt(0);
            }

            ComparisonMatches.Add(match.ToString());
        }

        if (IsComparing)
        {
            ComparisonStatus = $"Сравнение выполняется. Найдено совпадений: {MatchCount:N0}.";
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
        CompareCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanSaveMatrix));
        OnPropertyChanged(nameof(CanSelectComparisonFiles));
    }

    partial void OnCurrentOperationChanged(string value)
    {
        OnPropertyChanged(nameof(GenerateButtonText));
        OnPropertyChanged(nameof(SortButtonText));
    }

    partial void OnIsComparingChanged(bool value)
    {
        OnPropertyChanged(nameof(CompareButtonText));
        OnPropertyChanged(nameof(CanSaveMatrix));
        OnPropertyChanged(nameof(CanSelectComparisonFiles));
        GenerateCommand.NotifyCanExecuteChanged();
        SortColumnsCommand.NotifyCanExecuteChanged();
        CompareCommand.NotifyCanExecuteChanged();
        CancelComparisonCommand.NotifyCanExecuteChanged();
    }

    partial void OnFirstMatrixPathChanged(string value) => CompareCommand.NotifyCanExecuteChanged();

    partial void OnSecondMatrixPathChanged(string value) => CompareCommand.NotifyCanExecuteChanged();

    public void Dispose()
    {
        ErrorsChanged -= OnErrorsChanged;
        comparisonCancellation?.Cancel();
        lifetimeCancellation.Cancel();
        comparisonCancellation?.Dispose();
        lifetimeCancellation.Dispose();
    }
}

public sealed class MatrixPresentationChangedEventArgs(
    MatrixDescriptor descriptor,
    int[]? columnOrder) : EventArgs
{
    public MatrixDescriptor Descriptor { get; } = descriptor;
    public int[]? ColumnOrder { get; } = columnOrder;
}
