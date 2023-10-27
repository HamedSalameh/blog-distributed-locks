namespace Demo4_ZooKeeperDistributedLock.WebAPI.LockProvider
{
    public interface IDistributedLockProvider
    {
        void Close();
        bool Connect();
        string CreateLock(string path);
        Task<string[]> GetChildrenAsync(string path, bool order = false);
        Task<string[]> GetChildrenByAbsolutePathAsync(string absolutePath, bool order = false);
        bool Lock(string path);
        Task<bool> LockAsync(string path);
    }
}