namespace Demo3_ZooKeeperDistributedLock.WebAPI
{
    public class ZooKeeperConfiguration
    {
        public string ConnectionString { get; set; }
        public string LockPath { get; set; } = "distributed_locks"; // The default path for locks
        public double ConnectionTimeout { get; set; }
    }
}
