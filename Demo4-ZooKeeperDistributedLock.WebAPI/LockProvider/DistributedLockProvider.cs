using Demo4_ZooKeeperDistributedLock.WebAPI.InventoryService;
using Microsoft.Extensions.Options;
using org.apache.zookeeper;
using org.apache.zookeeper.data;
using System.Text;

namespace Demo4_ZooKeeperDistributedLock.WebAPI.LockProvider
{
    /* 
     CreateLock : Creates the lock node, that will be used as a lock later (this is not the actual lock)
     Syntax:
     
    string path = zookeeperLocker.CreateLock();
     if(zookeeperLocker.Lock(path)) {
        // Do something
        
     }

    */
    public class DistributedLockProvider : IDistributedLockProvider, IDisposable
    {
        // ZooKeeper client instance (ZooKeeperNetEx)
        ZooKeeper zookeeper;

        AutoResetEvent are = new AutoResetEvent(false);
        DefaultWatcher defaultWatcher;

        public Encoding Encoding { get; set; } = Encoding.Default;
        public bool Connected { get { return zookeeper != null && (zookeeper.getState() == ZooKeeper.States.CONNECTED || zookeeper.getState() == ZooKeeper.States.CONNECTEDREADONLY); } }
        public bool CanWrite { get { return zookeeper != null && zookeeper.getState() == ZooKeeper.States.CONNECTED; } }
        List<ACL> defaultACL = ZooDefs.Ids.OPEN_ACL_UNSAFE;
        public event Action OnConnected;
        public event Action OnDisposing;
        public string CurrentPath { get; private set; }

        // Default session timeout 
        public int SessionTimeout { get; private set; } = 10000;
        public string Address { get; set; }
        string pathSeperator = "/";

        // Basic node watcher class
        private class NullWatcher : Watcher
        {
            public static readonly NullWatcher Instance = new();

            private NullWatcher() { }

            public override Task process(WatchedEvent @event) => Task.CompletedTask;
        }

        // ctor with IOptions
        public DistributedLockProvider(IOptions<DistributedLockOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrEmpty(options.Value.Address))
            {
                throw new ArgumentNullException(nameof(options.Value.Address));
            }

            if (options.Value.SessionTimeout < 1000)
            {
                throw new ArgumentOutOfRangeException("sessionTimeout must be greater than 1000");
            }

            Address = options.Value.Address;
            SessionTimeout = options.Value.SessionTimeout;
            CurrentPath = pathSeperator;
        }

        public DistributedLockProvider(string address, int sessionTimeout = 10000)
        {
            if (sessionTimeout < 1000)
            {
                throw new ArgumentOutOfRangeException("sessionTimeout must be greater than 1000");
            }

            Address = address;
            SessionTimeout = sessionTimeout;
            CurrentPath = pathSeperator;
        }

        public bool Connect()
        {
            if (Connected)
            {
                return true;
            }
            if (zookeeper == null)
            {
                lock (this)
                {
                    defaultWatcher = defaultWatcher ?? new DefaultWatcher(are);
                    are.Reset();
                    zookeeper = new ZooKeeper(Address, SessionTimeout, defaultWatcher);
                    are.WaitOne(SessionTimeout);
                }
            }
            if (!Connected)
            {
                return false;
            }
            OnConnected?.Invoke();

            return true;
        }

        // Close ZooKeeper connection
        public void Close()
        {
            if (Connected)
            {
                zookeeper.closeAsync().Wait();
            }
        }

        /// <summary>
        /// Used to create a lock node
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public string CreateLock(string path)
        {
            if (path.Contains("/"))
            {
                throw new ArgumentException("invalid path");
            }
            return SetData(path, "", false, true);
        }

        /// <summary>
        /// Create the lock on the provide node path
        /// This lock should be released by calling the Unlock method or by disposing the object
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool Lock(string path)
        {
            return LockAsync(path).GetAwaiter().GetResult();
        }

        public async Task<bool> LockAsync(string path)
        {
            var array = await GetChildrenAsync("", true);
            if (array != null && array.Length > 0)
            {
                var first = array.FirstOrDefault();
                if (first == path)
                {
                    return true;
                }

                var index = array.ToList().IndexOf(path);
                if (index > 0)
                {
                    var are = new AutoResetEvent(false);
                    //var watcher = new NodeWatcher();
                    //watcher.NodeDeleted += (ze) =>
                    //{
                    //    are.Set();
                    //};
                    //if (await zookeeperHelper.WatchAsync(array[index - 1], watcher))
                    //{
                    //    if (!are.WaitOne(millisecondsTimeout))
                    //    {
                    //        return false;
                    //    }
                    //}

                    are.Dispose();
                }
                else
                {
                    throw new InvalidOperationException($"no locker found in path");
                }
            }
            return true;
        }

        public async Task<string[]> GetChildrenAsync(string path, bool order = false)
        {
            path = Combine(CurrentPath, path);
            return await GetChildrenByAbsolutePathAsync(path, order);
        }

        public async Task<string[]> GetChildrenByAbsolutePathAsync(string absolutePath, bool order = false)
        {
            var result = await zookeeper.getChildrenAsync(absolutePath, false);
            if (!order)
            {
                return result.Children.ToArray();
            }

            List<(string, long)> list = new List<(string, long)>();
            foreach (var child in result.Children)
            {
                var p = Combine(absolutePath, child);
                var stat = await zookeeper.existsAsync(p, false);
                if (stat != null)
                {
                    list.Add((child, stat.getCtime()));
                }
            }

            return list.OrderBy(l => l.Item2).Select(l => l.Item1).ToArray();
        }

        #region SetData - For node creation and writing data to ZK
        public string SetData(string value, bool persistent = false, bool sequential = false)
        {
            return SetDataAsync(value, persistent, sequential).GetAwaiter().GetResult();
        }
        public string SetData(string path, string value, bool persistent = false, bool sequential = false)
        {
            return SetDataAsync(path, value, persistent, sequential).GetAwaiter().GetResult();
        }
        public async Task<string> SetDataAsync(string value, bool persistent = false, bool sequential = false)
        {
            return await SetDataByAbsolutePathAsync(CurrentPath, value, persistent, sequential);
        }
        public async Task<string> SetDataAsync(string path, string value, bool persistent = false, bool sequential = false)
        {
            path = Combine(CurrentPath, path);
            return await SetDataByAbsolutePathAsync(path, value, persistent, sequential);
        }
        public async Task<string> SetDataByAbsolutePathAsync(string absolutePath, string value, bool persistent = false, bool sequential = false)
        {
            if (!Connected)
            {
                throw new Exception("Connection is not connected");
            }
            if (!CanWrite)
            {
                throw new Exception("Connection is readonly mode");
            }

            absolutePath = Combine(absolutePath);

            var splits = absolutePath.Split(new string[] { pathSeperator }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < splits.Length - 1; i++)
            {
                var path = Combine(splits.Take(i + 1).ToArray());
                if (await zookeeper.existsAsync(path, false) == null)
                {
                    await zookeeper.createAsync(path, new byte[0], defaultACL, persistent ?
                       sequential ? CreateMode.PERSISTENT_SEQUENTIAL : CreateMode.PERSISTENT :
                       sequential ? CreateMode.EPHEMERAL_SEQUENTIAL : CreateMode.EPHEMERAL);
                }
            }


            if (await zookeeper.existsAsync(absolutePath, false) == null)
            {
                absolutePath = await zookeeper.createAsync(absolutePath, Encoding.GetBytes(value), defaultACL, persistent ?
                    sequential ? CreateMode.PERSISTENT_SEQUENTIAL : CreateMode.PERSISTENT :
                    sequential ? CreateMode.EPHEMERAL_SEQUENTIAL : CreateMode.EPHEMERAL);
            }
            else
            {
                await zookeeper.setDataAsync(absolutePath, Encoding.GetBytes(value));
            }
            return absolutePath.Split(new string[] { pathSeperator }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
        }
        #endregion

        #region Node Exists Checks
        public async Task<bool> ExistsAsync()
        {
            return await ExistsByAbsolutePathAsync(CurrentPath);
        }

        public async Task<bool> ExistsAsync(string path)
        {
            path = Combine(CurrentPath, path);
            return await ExistsByAbsolutePathAsync(path);
        }

        public async Task<bool> ExistsByAbsolutePathAsync(string absolutePath)
        {
            absolutePath = Combine(absolutePath);
            return await zookeeper.existsAsync(absolutePath, false) != null;
        }
        #endregion

        private string Combine(params string[] paths)
        {
            List<string> list = new List<string>();
            foreach (var path in paths)
            {
                var ps = path.Split(new string[] { "/", "\\" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in ps)
                {
                    if (p == ".")
                    {
                        continue;
                    }
                    else if (p == "..")
                    {
                        if (list.Count == 0)
                        {
                            throw new ArgumentOutOfRangeException("path is out of range");
                        }

                        list.RemoveAt(list.Count - 1);
                    }
                    else
                    {
                        list.Add(p);
                    }
                }
            }

            return pathSeperator + string.Join(pathSeperator, list.ToArray());
        }

        public void Dispose()
        {
            OnDisposing?.Invoke();
            Close();
            are?.Dispose();
            GC.Collect();
        }
    }
}
