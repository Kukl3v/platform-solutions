using System.Globalization;
using Client.Models;

namespace Client.Services;

public sealed class MatrixColumnSorter
{
    public int[] Sort(
        MatrixDescriptor descriptor,
        CancellationToken cancellationToken)
    {
        return descriptor.NumberKind == MatrixNumberKind.Integer
            ? SortIntegers(descriptor, cancellationToken)
            : SortFractional(descriptor, cancellationToken);
    }

    private static int[] SortIntegers(
        MatrixDescriptor descriptor,
        CancellationToken cancellationToken)
    {
        var minima = new long[descriptor.Columns];
        ScanRows(descriptor, cancellationToken, (token, column, row) =>
        {
            if (!long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            {
                throw new InvalidDataException($"Некорректное целое значение в строке {row + 1}.");
            }

            if (row == 0 || value < minima[column])
            {
                minima[column] = value;
            }
        });

        return CreateStableOrder(descriptor.Columns, (left, right) => minima[left].CompareTo(minima[right]));
    }

    private static int[] SortFractional(
        MatrixDescriptor descriptor,
        CancellationToken cancellationToken)
    {
        var minima = new double[descriptor.Columns];
        ScanRows(descriptor, cancellationToken, (token, column, row) =>
        {
            if (!double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ||
                !double.IsFinite(value))
            {
                throw new InvalidDataException($"Некорректное дробное значение в строке {row + 1}.");
            }

            if (row == 0 || value < minima[column])
            {
                minima[column] = value;
            }
        });

        return CreateStableOrder(descriptor.Columns, (left, right) => minima[left].CompareTo(minima[right]));
    }

    private static void ScanRows(
        MatrixDescriptor descriptor,
        CancellationToken cancellationToken,
        SpanConsumer consume)
    {
        using var stream = new FileStream(
            descriptor.FilePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete,
            1024 * 1024,
            FileOptions.SequentialScan);
        using var reader = new StreamReader(stream);

        for (var row = 0; row < descriptor.Rows; row++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = reader.ReadLine() ??
                       throw new InvalidDataException($"Файл завершился до строки {row + 1}.");
            var span = line.AsSpan();
            var start = 0;
            var column = 0;

            for (var index = 0; index <= span.Length; index++)
            {
                if (index != span.Length && span[index] != '\t')
                {
                    continue;
                }

                if (column >= descriptor.Columns)
                {
                    throw new InvalidDataException($"Слишком много значений в строке {row + 1}.");
                }

                consume(span[start..index], column, row);
                column++;
                start = index + 1;
            }

            if (column != descriptor.Columns)
            {
                throw new InvalidDataException(
                    $"Строка {row + 1} содержит {column} значений вместо {descriptor.Columns}.");
            }

        }
    }

    private static int[] CreateStableOrder(int columnCount, Comparison<int> compareValues)
    {
        var order = Enumerable.Range(0, columnCount).ToArray();
        Array.Sort(order, (left, right) =>
        {
            var comparison = compareValues(left, right);
            return comparison != 0 ? comparison : left.CompareTo(right);
        });
        return order;
    }

    private delegate void SpanConsumer(ReadOnlySpan<char> token, int column, int row);
}
