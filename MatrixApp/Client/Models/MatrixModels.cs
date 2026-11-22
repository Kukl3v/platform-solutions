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

