using Medallion.Threading;

namespace Demo3_ZooKeeperDistributedLock.WebAPI
{

    public class ZooKeeperLockManager
    {
        
        private readonly IDistributedLockProvider distributedLockProvider;

        public ZooKeeperLockManager(IDistributedLockProvider distributedLockProvider)
        {
            this.distributedLockProvider = distributedLockProvider;
        }

        
    }
}
