using System.Net;

namespace Demo4_ZooKeeperDistributedLock.WebAPI.LockProvider
{
    public class DistributedLockOptions
    {
        public string Address { get; set; }
        public string LockPath { get; set; }
        public int SessionTimeout { get; set; }
    }
}
