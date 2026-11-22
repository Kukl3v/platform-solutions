using System.Buffers;
using System.Globalization;
using System.Text;
using Client.Models;

namespace Client.Services;

public sealed class MatrixStorageService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private readonly string storageDirectory;
    private readonly string matrixPath;

    public MatrixStorageService(string? storageDirectory = null)
    {
        this.storageDirectory = storageDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MatrixApp");
        matrixPath = Path.Combine(this.storageDirectory, "matrix.txt");
    }

    public MatrixDescriptor Generate(
        MatrixGenerationOptions options,
        CancellationToken cancellationToken)
    {
        ValidateDimensions(options.Rows, options.Columns);
        Directory.CreateDirectory(storageDirectory);

        var temporaryPath = Path.Combine(storageDirectory, $"matrix-{Guid.NewGuid():N}.tmp");
        var offsets = new long[options.Rows + 1];

        try
        {
            using (var stream = new FileStream(
                       temporaryPath,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None,
                       1024 * 1024,
                       FileOptions.SequentialScan))
            using (var writer = new StreamWriter(stream, Utf8WithoutBom, 1024 * 1024, leaveOpen: false))
            {
                writer.NewLine = "\n";
                var random = new Random();
                var line = new StringBuilder(Math.Min(options.Columns * 12, 1_000_000));
                long byteOffset = 0;

                for (var row = 0; row < options.Rows; row++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    offsets[row] = byteOffset;
                    line.Clear();

                    for (var column = 0; column < options.Columns; column++)
                    {
                        if (column > 0)
                        {
                            line.Append('\t');
                        }

                        if (options.NumberKind == MatrixNumberKind.Integer)
                        {
                            line.Append(NextInt64Inclusive(
                                    random,
                                    options.IntegerMinimum,
                                    options.IntegerMaximum)
                                .ToString(CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            line.Append(NextDoubleInclusive(
                                    random,
                                    options.FractionalMinimum,
                                    options.FractionalMaximum)
                                .ToString("R", CultureInfo.InvariantCulture));
                        }
                    }

                    var text = line.ToString();
                    writer.Write(text);
                    writer.Write('\n');
                    byteOffset = checked(byteOffset + Utf8WithoutBom.GetByteCount(text) + 1L);

                }

                offsets[options.Rows] = byteOffset;
            }

            cancellationToken.ThrowIfCancellationRequested();
            File.Move(temporaryPath, matrixPath, overwrite: true);
            return new MatrixDescriptor(
                matrixPath,
                options.Rows,
                options.Columns,
                options.NumberKind,
                offsets);
        }
        catch
        {
            TryDelete(temporaryPath);
            throw;
        }
    }

    public string[] ReadRow(
        MatrixDescriptor descriptor,
        int rowIndex,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(rowIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(rowIndex, descriptor.Rows);

        var byteCount64 = descriptor.RowOffsets[rowIndex + 1] - descriptor.RowOffsets[rowIndex];
        if (byteCount64 <= 0 || byteCount64 > int.MaxValue)
        {
            throw new InvalidDataException("Некорректная длина строки матрицы.");
        }

        var byteCount = (int)byteCount64;
        var buffer = ArrayPool<byte>.Shared.Rent(byteCount);
        try
        {
            using var stream = new FileStream(
                descriptor.FilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete,
                64 * 1024,
                FileOptions.RandomAccess);
            stream.Position = descriptor.RowOffsets[rowIndex];
            stream.ReadExactly(buffer.AsSpan(0, byteCount));
            cancellationToken.ThrowIfCancellationRequested();

            var textLength = byteCount;
            while (textLength > 0 && buffer[textLength - 1] is (byte)'\n' or (byte)'\r')
            {
                textLength--;
            }

            var values = Utf8WithoutBom.GetString(buffer, 0, textLength).Split('\t');
            if (values.Length != descriptor.Columns)
            {
                throw new InvalidDataException(
                    $"Строка {rowIndex + 1} содержит {values.Length} значений вместо {descriptor.Columns}.");
            }

            return values;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public string ReadValue(
        MatrixDescriptor descriptor,
        int rowIndex,
        int columnIndex,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(rowIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(rowIndex, descriptor.Rows);
        ArgumentOutOfRangeException.ThrowIfNegative(columnIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(columnIndex, descriptor.Columns);

        var byteCount64 = descriptor.RowOffsets[rowIndex + 1] - descriptor.RowOffsets[rowIndex];
        if (byteCount64 <= 0 || byteCount64 > int.MaxValue)
        {
            throw new InvalidDataException("Некорректная длина строки матрицы.");
        }

        var byteCount = (int)byteCount64;
        var buffer = ArrayPool<byte>.Shared.Rent(byteCount);
        try
        {
            using var stream = new FileStream(
                descriptor.FilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete,
                64 * 1024,
                FileOptions.RandomAccess);
            stream.Position = descriptor.RowOffsets[rowIndex];
            stream.ReadExactly(buffer.AsSpan(0, byteCount));
            cancellationToken.ThrowIfCancellationRequested();

            var currentColumn = 0;
            var valueStart = 0;
            for (var index = 0; index <= byteCount; index++)
            {
                var atEnd = index == byteCount || buffer[index] is (byte)'\n' or (byte)'\r';
                if (!atEnd && buffer[index] != (byte)'\t')
                {
                    continue;
                }

                if (currentColumn == columnIndex)
                {
                    return Utf8WithoutBom.GetString(buffer, valueStart, index - valueStart);
                }

                if (atEnd)
                {
                    break;
                }

                currentColumn++;
                valueStart = index + 1;
            }

            throw new InvalidDataException(
                $"В строке {rowIndex + 1} отсутствует столбец {columnIndex + 1}.");
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public async Task CopyMatrixAsync(
        MatrixDescriptor descriptor,
        string destinationPath,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        var sourcePath = Path.GetFullPath(descriptor.FilePath);
        var targetPath = Path.GetFullPath(destinationPath);
        if (string.Equals(sourcePath, targetPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Выберите файл, отличный от рабочего файла матрицы.");
        }

        var targetDirectory = Path.GetDirectoryName(targetPath) ??
                              throw new ArgumentException("Не удалось определить папку назначения.");
        Directory.CreateDirectory(targetDirectory);
        var temporaryPath = Path.Combine(targetDirectory, $".{Path.GetFileName(targetPath)}-{Guid.NewGuid():N}.tmp");

        try
        {
            await using (var source = new FileStream(
                             sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                             1024 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan))
            await using (var destination = new FileStream(
                             temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                             1024 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                await source.CopyToAsync(destination, cancellationToken);
                await destination.FlushAsync(cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
            File.Move(temporaryPath, targetPath, overwrite: true);
        }
        catch
        {
            TryDelete(temporaryPath);
            throw;
        }
    }

    internal void ScanRows(
        string filePath,
        CancellationToken cancellationToken,
        MatrixRowVisitor visitor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(visitor);

        using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            1024 * 1024,
            FileOptions.SequentialScan);

        var readBuffer = ArrayPool<byte>.Shared.Rent(1024 * 1024);
        var continuedLine = new ArrayBufferWriter<byte>();
        long absoluteOffset = 0;
        long lineOffset = 0;
        var rowNumber = 0;

        try
        {
            int bytesRead;
            while ((bytesRead = stream.Read(readBuffer, 0, readBuffer.Length)) > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var block = readBuffer.AsSpan(0, bytesRead);
                var segmentStart = 0;

                for (var index = 0; index < block.Length; index++)
                {
                    if (block[index] != (byte)'\n')
                    {
                        continue;
                    }

                    var segment = block[segmentStart..index];
                    if (continuedLine.WrittenCount == 0)
                    {
                        VisitRow(segment, lineOffset, ref rowNumber, cancellationToken, visitor);
                    }
                    else
                    {
                        Append(continuedLine, segment);
                        VisitRow(continuedLine.WrittenSpan, lineOffset, ref rowNumber, cancellationToken, visitor);
                        continuedLine.Clear();
                    }

                    segmentStart = index + 1;
                    lineOffset = absoluteOffset + segmentStart;
                }

                if (segmentStart < block.Length)
                {
                    Append(continuedLine, block[segmentStart..]);
                }

                absoluteOffset += bytesRead;
            }

            if (continuedLine.WrittenCount > 0)
            {
                VisitRow(continuedLine.WrittenSpan, lineOffset, ref rowNumber, cancellationToken, visitor);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(readBuffer);
        }
    }

    internal FileStream OpenRandomAccessReader(string filePath) => new(
        filePath,
        FileMode.Open,
        FileAccess.Read,
        FileShare.Read,
        64 * 1024,
        FileOptions.RandomAccess);

    internal static byte[] ReadRowBytes(FileStream stream, long offset, int byteCount)
    {
        var bytes = new byte[byteCount];
        stream.Position = offset;
        stream.ReadExactly(bytes);
        return bytes;
    }

    private static void VisitRow(
        ReadOnlySpan<byte> row,
        long offset,
        ref int rowNumber,
        CancellationToken cancellationToken,
        MatrixRowVisitor visitor)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!row.IsEmpty && row[^1] == (byte)'\r')
        {
            row = row[..^1];
        }

        var adjustedOffset = offset;
        if (rowNumber == 0 && row.StartsWith(new byte[] { 0xEF, 0xBB, 0xBF }))
        {
            row = row[3..];
            adjustedOffset += 3;
        }

        rowNumber++;
        visitor(row, adjustedOffset, row.Length, rowNumber);
    }

    private static void Append(ArrayBufferWriter<byte> writer, ReadOnlySpan<byte> value)
    {
        value.CopyTo(writer.GetSpan(value.Length));
        writer.Advance(value.Length);
    }

    private static long NextInt64Inclusive(Random random, long minimum, long maximum)
    {
        if (minimum > maximum)
        {
            throw new ArgumentException("Минимум не может превышать максимум.");
        }

        if (minimum == maximum)
        {
            return minimum;
        }

        if (maximum < long.MaxValue)
        {
            return random.NextInt64(minimum, maximum + 1);
        }

        if (minimum > long.MinValue)
        {
            return random.NextInt64(minimum - 1, maximum) + 1;
        }

        Span<byte> bytes = stackalloc byte[sizeof(long)];
        random.NextBytes(bytes);
        return BitConverter.ToInt64(bytes);
    }

    private static double NextDoubleInclusive(Random random, double minimum, double maximum)
    {
        if (!double.IsFinite(minimum) || !double.IsFinite(maximum) || minimum > maximum)
        {
            throw new ArgumentException("Недопустимый диапазон дробных значений.");
        }

        if (minimum == maximum)
        {
            return minimum;
        }

        var width = maximum - minimum;
        if (!double.IsFinite(width))
        {
            throw new ArgumentException("Диапазон дробных значений слишком велик.");
        }

        const long steps = 1L << 53;
        var step = random.NextInt64(steps + 1);
        return step == steps ? maximum : minimum + width * (step / (double)steps);
    }

    private static void ValidateDimensions(int rows, int columns)
    {
        if (rows is < 1 or > 20_000 || columns is < 1 or > 20_000)
        {
            throw new ArgumentOutOfRangeException(
                nameof(rows),
                "Размеры матрицы должны находиться в диапазоне от 1 до 20 000.");
        }

        _ = checked((long)rows * columns);
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}

internal delegate void MatrixRowVisitor(
    ReadOnlySpan<byte> row,
    long byteOffset,
    int byteCount,
    int rowNumber);

