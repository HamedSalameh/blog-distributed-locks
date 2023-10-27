using org.apache.zookeeper;

namespace Demo4_ZooKeeperDistributedLock.WebAPI.LockProvider
{
    public class DefaultWatcher : Watcher
    {
        EventWaitHandle ewh;

        public DefaultWatcher(EventWaitHandle ewh)
        {
            this.ewh = ewh;
        }

        public override Task process(WatchedEvent @event)
        {
            var state = @event.getState();
            if (state == Event.KeeperState.ConnectedReadOnly || state == Event.KeeperState.SyncConnected)
            {
                ewh.Set();
            }

            return Task.FromResult(1);
        }
    }
}
