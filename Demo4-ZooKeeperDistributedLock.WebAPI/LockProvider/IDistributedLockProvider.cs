namespace Demo4_ZooKeeperDistributedLock.WebAPI.LockProvider
{
    public interface IDistributedLockProvider
    {
        Task<bool> AcquireLockAsync(string path, CancellationToken cancellationToken);
        void Close();
        bool Connect(CancellationToken cancellationToken);
        string CreateLock(string path);
        Task<string[]> GetChildrenAsync(string path, bool order = false);
        Task<string[]> GetChildrenByAbsolutePathAsync(string absolutePath, bool order = false);
        bool Lock(string path, CancellationToken cancellationToken);
        Task<bool> LockAsync(string path, CancellationToken cancellationToken);
    }
}