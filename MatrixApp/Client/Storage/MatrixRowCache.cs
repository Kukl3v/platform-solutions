using System.Threading.Channels;
using Client.Models;
using Client.Services;

namespace Client.Storage;

internal sealed class MatrixRowCache : IDisposable
{
    private const int MaximumCachedRows = 32;
    private readonly MatrixStorageService storage;
    private readonly MatrixDescriptor descriptor;
    private readonly Channel<int> requests = Channel.CreateBounded<int>(new BoundedChannelOptions(64)
    {
        FullMode = BoundedChannelFullMode.Wait,
        SingleReader = true,
        SingleWriter = false
    });
    private readonly Dictionary<int, CacheEntry> rows = [];
    private readonly HashSet<int> pendingRows = [];
    private readonly LinkedList<int> lru = [];
    private readonly object sync = new();
    private readonly CancellationTokenSource cancellation = new();
    private readonly Task worker;
    private bool disposed;

    public MatrixRowCache(MatrixStorageService storage, MatrixDescriptor descriptor)
    {
        this.storage = storage;
        this.descriptor = descriptor;
        worker = Task.Run(ProcessRequestsAsync);
    }

    public event EventHandler<int>? RowLoaded;

    public bool TryGetValue(int row, int column, out string value)
    {
        lock (sync)
        {
            if (rows.TryGetValue(row, out var entry))
            {
                lru.Remove(entry.LruNode);
                lru.AddFirst(entry.LruNode);
                value = entry.Values[column];
                return true;
            }

            if (!disposed && pendingRows.Add(row) && !requests.Writer.TryWrite(row))
            {
                pendingRows.Remove(row);
            }
        }

        value = string.Empty;
        return false;
    }

    private async Task ProcessRequestsAsync()
    {
        try
        {
            await foreach (var row in requests.Reader.ReadAllAsync(cancellation.Token))
            {
                string[]? values = null;
                try
                {
                    values = storage.ReadRow(descriptor, row, cancellation.Token);
                }
                catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
                {
                    return;
                }
                catch (IOException)
                {
                }
                catch (InvalidDataException)
                {
                }

                lock (sync)
                {
                    pendingRows.Remove(row);
                    if (values is not null && !disposed)
                    {
                        Add(row, values);
                    }
                }

                if (values is not null)
                {
                    RowLoaded?.Invoke(this, row);
                }
            }
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
        }
    }

    private void Add(int row, string[] values)
    {
        if (rows.Count == MaximumCachedRows)
        {
            var last = lru.Last!;
            rows.Remove(last.Value);
            lru.RemoveLast();
        }

        var node = lru.AddFirst(row);
        rows[row] = new CacheEntry(values, node);
    }

    public void Dispose()
    {
        lock (sync)
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            rows.Clear();
            pendingRows.Clear();
            lru.Clear();
        }

        requests.Writer.TryComplete();
        cancellation.Cancel();
        cancellation.Dispose();
    }

    private sealed record CacheEntry(string[] Values, LinkedListNode<int> LruNode);
}

