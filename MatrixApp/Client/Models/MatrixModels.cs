namespace Client.Models;

public enum MatrixNumberKind
{
    Integer,
    Fractional
}

public sealed record MatrixGenerationOptions(
    int Rows,
    int Columns,
    MatrixNumberKind NumberKind,
    long IntegerMinimum,
    long IntegerMaximum,
    double FractionalMinimum,
    double FractionalMaximum);

public sealed record MatrixDescriptor(
    string FilePath,
    int Rows,
    int Columns,
    MatrixNumberKind NumberKind,
    long[] RowOffsets);

public sealed record MatrixMatch(int FirstRowNumber, int SecondRowNumber)
{
    public override string ToString() =>
        $"Строка {FirstRowNumber} первой матрицы совпадает со строкой {SecondRowNumber} второй матрицы.";
}

public sealed record MatrixComparisonProgress(
    long TotalMatches,
    IReadOnlyList<MatrixMatch> RecentMatches);

public sealed record MatrixComparisonResult(
    int FirstRowCount,
    int SecondRowCount,
    int FirstColumnCount,
    int SecondColumnCount,
    long MatchCount)
{
    public bool ColumnCountsDiffer => FirstColumnCount != SecondColumnCount;
}

