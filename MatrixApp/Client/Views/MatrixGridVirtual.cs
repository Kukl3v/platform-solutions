using Client.Models;
using Client.Services;
using SourceGrid;
using SourceGrid.Cells;
using SourceGrid.Cells.Models;

namespace Client.Views;

public sealed class MatrixGridVirtual : GridVirtual
{
    private readonly MatrixStorageService storage;
    private readonly ICellVirtual valueCell;
    private MatrixDescriptor? descriptor;
    private int[]? columnOrder;

    public MatrixGridVirtual(MatrixStorageService storage)
    {
        this.storage = storage;
        DefaultHeight = 26;
        DefaultWidth = 90;
        FixedRows = 0;
        FixedColumns = 0;
        EnableSort = false;
        SelectionMode = GridSelectionMode.Cell;

        var cell = new SourceGrid.Cells.Virtual.CellVirtual();
        cell.Model.AddModel(new MatrixValueModel());
        cell.Editor = null;
        valueCell = cell;
    }

    public override bool EnableSort { get; set; }

    public void Bind(MatrixDescriptor matrix, int[]? displayedColumnOrder)
    {
        if (displayedColumnOrder is not null && displayedColumnOrder.Length != matrix.Columns)
        {
            throw new ArgumentException("Некорректная перестановка столбцов.", nameof(displayedColumnOrder));
        }

        descriptor = matrix;
        columnOrder = displayedColumnOrder;

        Rows.RowsChanged();
        Columns.ColumnsChanged();
        Invalidate();
    }

    internal string GetDisplayedValue(int row, int displayedColumn)
    {
        var actualColumn = columnOrder is null ? displayedColumn : columnOrder[displayedColumn];
        return descriptor is null
            ? string.Empty
            : storage.ReadValue(descriptor, row, actualColumn);
    }

    public override ICellVirtual GetCell(int row, int column) => valueCell;

    protected override RowsBase CreateRowsObject() => new MatrixRows(this);
    protected override ColumnsBase CreateColumnsObject() => new MatrixColumns(this);

    private sealed class MatrixRows(MatrixGridVirtual grid) : RowsSimpleBase(grid)
    {
        public override int Count => grid.descriptor?.Rows ?? 0;
        public override SourceGrid.AutoSizeMode GetAutoSizeMode(int row) => SourceGrid.AutoSizeMode.None;
    }

    private sealed class MatrixColumns(MatrixGridVirtual grid) : ColumnsSimpleBase(grid)
    {
        public override int Count => grid.descriptor?.Columns ?? 0;
        public override SourceGrid.AutoSizeMode GetAutoSizeMode(int column) => SourceGrid.AutoSizeMode.None;
    }

    private sealed class MatrixValueModel : IValueModel
    {
        public object GetValue(CellContext cellContext) =>
            ((MatrixGridVirtual)cellContext.Grid).GetDisplayedValue(
                cellContext.Position.Row,
                cellContext.Position.Column);

        public void SetValue(CellContext cellContext, object value) =>
            throw new NotSupportedException("Матрица доступна только для чтения.");
    }
}
