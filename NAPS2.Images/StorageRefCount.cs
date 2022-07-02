namespace NAPS2.Images;

public class RefCount
{
    private readonly IDisposable _disposable;
    private int _count;
    private bool _disposed;

    public RefCount(IDisposable disposable)
    {
        _disposable = disposable;
    }

    public Token NewToken() => new(this);

    public class Token : IDisposable
    {
        private bool _tokenDisposed;
        
        public Token(RefCount refCount)
        {
            if (refCount._disposed)
            {
                throw new ObjectDisposedException(nameof(refCount));
            }
            RefCount = refCount;
            lock (RefCount)
            {
                RefCount._count++;
            }
        }

        public RefCount RefCount { get; }

        public void Dispose()
        {
            bool disposing = false;
            lock (this)
            {
                if (_tokenDisposed)
                {
                    return;
                }
                lock (RefCount)
                {
                    _tokenDisposed = true;
                    RefCount._count--;
                    if (RefCount._count <= 0)
                    {
                        disposing = true;
                        RefCount._disposed = true;
                    }
                }
            }
            if (disposing)
            {
                RefCount._disposable.Dispose();
            }
        }
    }

}
