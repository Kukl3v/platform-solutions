using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using Client.Models;

namespace Client.Services;

public sealed class MatrixComparisonService(MatrixStorageService storage)
{
    private const int MaximumProgressMatches = 1000;
    private static readonly TimeSpan ProgressInterval = TimeSpan.FromMilliseconds(100);

    public MatrixComparisonResult Compare(
        string firstFilePath,
        string secondFilePath,
        IProgress<MatrixComparisonProgress>? progress,
        CancellationToken cancellationToken)
    {
        ValidatePath(firstFilePath, "первой");
        ValidatePath(secondFilePath, "второй");

        var index = new Dictionary<RowHash, List<RowLocation>>();
        var canonicalRow = new ArrayBufferWriter<byte>();
        var firstRows = 0;
        var firstColumns = 0;
        using var firstFileReader = storage.OpenRandomAccessReader(firstFilePath);

        storage.ScanRows(firstFilePath, cancellationToken, (row, offset, byteCount, rowNumber) =>
        {
            var columns = CanonicalizeRow(row, canonicalRow, firstFilePath, rowNumber);
            firstColumns = ValidateColumnCount(firstColumns, columns, firstFilePath, rowNumber);
            firstRows = rowNumber;

            var hash = RowHash.Create(canonicalRow.WrittenSpan);
            if (!index.TryGetValue(hash, out var locations))
            {
                locations = [];
                index.Add(hash, locations);
            }

            locations.Add(new RowLocation(rowNumber, offset, byteCount));
        });

        if (firstRows == 0)
        {
            throw new InvalidDataException($"Файл «{firstFilePath}» не содержит строк матрицы.");
        }

        var recentMatches = new Queue<MatrixMatch>(MaximumProgressMatches);
        var stopwatch = Stopwatch.StartNew();
        var lastReport = TimeSpan.Zero;
        var secondRows = 0;
        var secondColumns = 0;
        long matchCount = 0;

        try
        {
            storage.ScanRows(secondFilePath, cancellationToken, (row, _, _, rowNumber) =>
            {
                var columns = CanonicalizeRow(row, canonicalRow, secondFilePath, rowNumber);
                secondColumns = ValidateColumnCount(secondColumns, columns, secondFilePath, rowNumber);
                secondRows = rowNumber;

                if (columns == firstColumns)
                {
                    var hash = RowHash.Create(canonicalRow.WrittenSpan);
                    if (index.TryGetValue(hash, out var candidates))
                    {
                        foreach (var candidate in candidates)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            var firstRow = MatrixStorageService.ReadRowBytes(
                                firstFileReader, candidate.ByteOffset, candidate.ByteCount);
                            var verifiedRow = new ArrayBufferWriter<byte>(firstRow.Length);
                            _ = CanonicalizeRow(
                                firstRow, verifiedRow, firstFilePath, candidate.RowNumber);

                            if (!canonicalRow.WrittenSpan.SequenceEqual(verifiedRow.WrittenSpan))
                            {
                                continue;
                            }

                            matchCount++;
                            if (recentMatches.Count == MaximumProgressMatches)
                            {
                                recentMatches.Dequeue();
                            }

                            recentMatches.Enqueue(new MatrixMatch(candidate.RowNumber, rowNumber));
                            if (stopwatch.Elapsed - lastReport >= ProgressInterval)
                            {
                                ReportProgress(progress, matchCount, recentMatches);
                                lastReport = stopwatch.Elapsed;
                            }
                        }
                    }
                }

                if (recentMatches.Count > 0 && stopwatch.Elapsed - lastReport >= ProgressInterval)
                {
                    ReportProgress(progress, matchCount, recentMatches);
                    lastReport = stopwatch.Elapsed;
                }
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            ReportProgress(progress, matchCount, recentMatches);
            throw;
        }

        if (secondRows == 0)
        {
            throw new InvalidDataException($"Файл «{secondFilePath}» не содержит строк матрицы.");
        }

        ReportProgress(progress, matchCount, recentMatches);
        return new MatrixComparisonResult(
            firstRows, secondRows, firstColumns, secondColumns, matchCount);
    }

    private static int CanonicalizeRow(
        ReadOnlySpan<byte> row,
        ArrayBufferWriter<byte> destination,
        string filePath,
        int rowNumber)
    {
        destination.Clear();
        var tokenStart = 0;
        var column = 0;

        for (var index = 0; index <= row.Length; index++)
        {
            if (index != row.Length && row[index] != (byte)'\t')
            {
                continue;
            }

            column++;
            try
            {
                AppendCanonicalNumber(row[tokenStart..index], destination);
            }
            catch (FormatException exception)
            {
                throw new InvalidDataException(
                    $"Некорректное число в файле «{filePath}», строка {rowNumber}, столбец {column}: {exception.Message}",
                    exception);
            }

            tokenStart = index + 1;
        }

        return column;
    }

    private static void AppendCanonicalNumber(
        ReadOnlySpan<byte> source,
        ArrayBufferWriter<byte> destination)
    {
        while (!source.IsEmpty && IsAsciiWhitespace(source[0]))
        {
            source = source[1..];
        }

        while (!source.IsEmpty && IsAsciiWhitespace(source[^1]))
        {
            source = source[..^1];
        }

        if (source.IsEmpty)
        {
            throw new FormatException("пустое значение");
        }

        AppendByte(destination, 0xFF);
        var index = 0;
        var negative = false;
        if (source[index] is (byte)'+' or (byte)'-')
        {
            negative = source[index] == (byte)'-';
            index++;
        }

        var hasDigit = false;
        var decimalSeen = false;
        var fractionalDigits = 0L;
        var significantStarted = false;
        var pendingZeros = 0L;

        for (; index < source.Length && source[index] is not (byte)'e' and not (byte)'E'; index++)
        {
            var current = source[index];
            if (current == (byte)'.' && !decimalSeen)
            {
                decimalSeen = true;
                continue;
            }

            if (current is < (byte)'0' or > (byte)'9')
            {
                throw new FormatException("ожидалось десятичное число в InvariantCulture");
            }

            hasDigit = true;
            if (decimalSeen)
            {
                fractionalDigits = checked(fractionalDigits + 1);
            }

            if (current == (byte)'0')
            {
                if (significantStarted)
                {
                    pendingZeros = checked(pendingZeros + 1);
                }

                continue;
            }

            if (!significantStarted)
            {
                significantStarted = true;
                if (negative)
                {
                    AppendByte(destination, (byte)'-');
                }
            }

            while (pendingZeros > 0)
            {
                AppendByte(destination, (byte)'0');
                pendingZeros--;
            }

            AppendByte(destination, current);
        }

        if (!hasDigit)
        {
            throw new FormatException("число не содержит цифр");
        }

        long explicitExponent = 0;
        if (index < source.Length)
        {
            index++;
            var exponentNegative = false;
            if (index < source.Length && source[index] is (byte)'+' or (byte)'-')
            {
                exponentNegative = source[index] == (byte)'-';
                index++;
            }

            var exponentStart = index;
            try
            {
                for (; index < source.Length; index++)
                {
                    if (source[index] is < (byte)'0' or > (byte)'9')
                    {
                        throw new FormatException("некорректная степень числа");
                    }

                    explicitExponent = checked(explicitExponent * 10 + source[index] - (byte)'0');
                }
            }
            catch (OverflowException)
            {
                throw new FormatException("слишком большая степень числа");
            }

            if (index == exponentStart)
            {
                throw new FormatException("после E должна быть указана степень");
            }

            if (exponentNegative)
            {
                explicitExponent = -explicitExponent;
            }
        }

        if (!significantStarted)
        {
            AppendByte(destination, (byte)'0');
            AppendInt64(destination, 0);
            return;
        }

        try
        {
            AppendInt64(destination, checked(explicitExponent - fractionalDigits + pendingZeros));
        }
        catch (OverflowException)
        {
            throw new FormatException("слишком большая степень числа");
        }
    }

    private static int ValidateColumnCount(
        int expected,
        int actual,
        string filePath,
        int rowNumber)
    {
        if (rowNumber == 1)
        {
            return actual;
        }

        if (actual != expected)
        {
            throw new InvalidDataException(
                $"Строка {rowNumber} файла «{filePath}» содержит {actual} значений вместо {expected}.");
        }

        return expected;
    }

    private static void ReportProgress(
        IProgress<MatrixComparisonProgress>? progress,
        long matchCount,
        Queue<MatrixMatch> recentMatches)
    {
        if (recentMatches.Count == 0)
        {
            progress?.Report(new MatrixComparisonProgress(matchCount, []));
            return;
        }

        progress?.Report(new MatrixComparisonProgress(matchCount, recentMatches.ToArray()));
        recentMatches.Clear();
    }

    private static bool IsAsciiWhitespace(byte value) => value is (byte)' ' or (byte)'\r' or (byte)'\n' or 0x0B or 0x0C;

    private static void AppendByte(ArrayBufferWriter<byte> writer, byte value)
    {
        writer.GetSpan(1)[0] = value;
        writer.Advance(1);
    }

    private static void AppendInt64(ArrayBufferWriter<byte> writer, long value)
    {
        BinaryPrimitives.WriteInt64LittleEndian(writer.GetSpan(sizeof(long)), value);
        writer.Advance(sizeof(long));
    }

    private static void ValidatePath(string path, string matrixName)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException($"Укажите путь к файлу {matrixName} матрицы.");
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Файл {matrixName} матрицы не найден.", path);
        }
    }

    private readonly record struct RowLocation(int RowNumber, long ByteOffset, int ByteCount);

    private readonly record struct RowHash(ulong First, ulong Second)
    {
        public static RowHash Create(ReadOnlySpan<byte> value)
        {
            Span<byte> hash = stackalloc byte[32];
            SHA256.HashData(value, hash);
            return new RowHash(
                BinaryPrimitives.ReadUInt64LittleEndian(hash),
                BinaryPrimitives.ReadUInt64LittleEndian(hash[8..]));
        }
    }
}
