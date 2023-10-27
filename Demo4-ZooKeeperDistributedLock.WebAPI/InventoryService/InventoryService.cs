
using Demo4_ZooKeeperDistributedLock.WebAPI.Controllers;
using Demo4_ZooKeeperDistributedLock.WebAPI.LockProvider;
using Microsoft.Extensions.Options;

namespace Demo4_ZooKeeperDistributedLock.WebAPI.InventoryService
{
    public class InventoryService : IInventoryService
    {
        // In-memory inventory mock
        private static List<InventoryItem> inventoryDB = new List<InventoryItem>
        {
            new InventoryItem { Id = 1, Name = "Item 1", StockCount = 10 },
            new InventoryItem { Id = 2, Name = "Item 2", StockCount = 20 },
            new InventoryItem { Id = 3, Name = "Item 3", StockCount = 30 },
            new InventoryItem { Id = 4, Name = "Item 4", StockCount = 40 },
            new InventoryItem { Id = 5, Name = "Item 5", StockCount = 50 },
        };

        private readonly ILogger<InventoryService> logger;
        private readonly IOptions<DistributedLockOptions> options;

        public InventoryService(ILogger<InventoryService> logger, IOptions<DistributedLockOptions> options)
        {
            this.logger = logger;
            this.options = options;
        }

        public async Task<InventoryItem> Add(InventoryItem item, CancellationToken cancellationToken)
        {
            logger.LogDebug($"Adding item {item}");

            //await distributedLockProvider.Lock();
            {
                var inventory = inventoryDB;
                if (inventory.Any(i => i.Id == item.Id))
                {
                    throw new Exception($"Item with id {item.Id} already exists");
                }

                inventory.Add(item);
                // demo update inventory
                await Task.Delay(500);

                //await distributedLockProvider.Unlock();
                return item;
            }
        }

        public async Task<InventoryItem> Get(int id, CancellationToken cancellationToken)
        {
            // get inventory item by id with lock protection
            logger.LogDebug($"Getting item with id {id}");
            //await distributedLockProvider.Lock();
            {

                var inventory = inventoryDB;
                var item = inventory.FirstOrDefault(i => i.Id == id);
                if (item == null)
                {
                    throw new Exception($"Item with id {id} does not exist");
                }

                //await distributedLockProvider.Unlock();
                return item;
            }
        }

        public Task<IEnumerable<InventoryItem>> GetAll(CancellationToken cancellationToken)
        {
            logger.LogDebug("Getting all items");
            var items = new List<InventoryItem>();
 
            using (DistributedLockProvider locker = new DistributedLockProvider(options))
            {
                locker.Connect();
                var _lock = locker.CreateLock("lock");
                locker.Lock(_lock);

                // get inventory items with lock protection
                items = inventoryDB;

            }

            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<IEnumerable<InventoryItem>>(cancellationToken);
            }

            return Task.FromResult<IEnumerable<InventoryItem>>(items);
        }
    }
}
