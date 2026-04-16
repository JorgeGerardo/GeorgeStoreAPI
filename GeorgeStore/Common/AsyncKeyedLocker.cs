using System.Collections.Concurrent;

public sealed class KeyedAsyncLock
{
    private sealed class LockEntry
    {
        public readonly SemaphoreSlim Semaphore = new(1, 1);
        public int RefCount = 0;
    }

    private readonly ConcurrentDictionary<string, LockEntry> _locks = new();

    public async ValueTask<IAsyncDisposable> AcquireAsync(
        string key,
        CancellationToken ct = default)
    {
        var entry = _locks.GetOrAdd(key, _ => new LockEntry());

        Interlocked.Increment(ref entry.RefCount);

        try
        {
            await entry.Semaphore.WaitAsync(ct).ConfigureAwait(false);
        }
        catch
        {
            ReleaseEntry(key, entry);
            throw;
        }

        return new Releaser(this, key, entry);
    }

    public async ValueTask<IAsyncDisposable> AcquireAsync(
        string key,
        TimeSpan timeout,
        CancellationToken ct = default)
    {
        var entry = _locks.GetOrAdd(key, _ => new LockEntry());

        Interlocked.Increment(ref entry.RefCount);

        bool entered = false;
        try
        {
            entered = await entry.Semaphore.WaitAsync(timeout, ct).ConfigureAwait(false);

            if (!entered)
                throw new TimeoutException($"Timeout acquiring lock for key '{key}'");

            return new Releaser(this, key, entry);
        }
        catch
        {
            if (!entered)
                ReleaseEntry(key, entry);
            throw;
        }
    }

    private void ReleaseEntry(string key, LockEntry entry)
    {
        if (Interlocked.Decrement(ref entry.RefCount) == 0)
            _locks.TryRemove(key, out _);
    }

    private sealed class Releaser : IAsyncDisposable
    {
        private readonly KeyedAsyncLock _owner;
        private readonly string _key;
        private readonly LockEntry _entry;
        private int _disposed;

        public Releaser(KeyedAsyncLock owner, string key, LockEntry entry)
        {
            _owner = owner;
            _key = key;
            _entry = entry;
        }

        public ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                _entry.Semaphore.Release();
                _owner.ReleaseEntry(_key, _entry);
            }

            return ValueTask.CompletedTask;
        }
    }

    public int CurrentKeyCount => _locks.Count;
}