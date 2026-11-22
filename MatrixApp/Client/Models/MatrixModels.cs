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

public readonly record struct MatrixProgress(int Completed, int Total, string Stage)
{
    public int Percentage => Total == 0 ? 0 : (int)((long)Completed * 100 / Total);
}

