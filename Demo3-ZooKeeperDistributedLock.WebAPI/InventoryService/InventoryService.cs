using Demo3_ZooKeeperDistributedLock.WebAPI.Controllers;
using Medallion.Threading;

namespace Demo3_ZooKeeperDistributedLock.WebAPI.InventoryService
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

        private readonly IDistributedLockProvider distributedLockProvider;
        private readonly ILogger<InventoryService> logger;

        public InventoryService(ILogger<InventoryService> logger, IDistributedLockProvider distributedLockProvider)
        {
            this.logger = logger;
            this.distributedLockProvider = distributedLockProvider;
        }

        public async Task<InventoryItem> Add(InventoryItem item, CancellationToken cancellationToken)
        {
            logger.LogDebug($"Adding item {item}");
            await using (await distributedLockProvider.TryAcquireLockAsync("inventory_lock", TimeSpan.FromSeconds(10), cancellationToken))
            {
                var inventory = inventoryDB;
                  if (inventory.Any(i => i.Id == item.Id))
                {
                    throw new Exception($"Item with id {item.Id} already exists");
                }

                inventory.Add(item);
                // demo update inventory
                await Task.Delay(500);

                return item;
            }
        }

        public async Task<InventoryItem> Get(int id, CancellationToken cancellationToken)
        {
            // get inventory item by id with lock protection
            logger.LogDebug($"Getting item with id {id}");
            await using (await distributedLockProvider.TryAcquireLockAsync("inventory_lock", TimeSpan.FromSeconds(10), cancellationToken))
            {

                var inventory = inventoryDB;
                var item = inventory.FirstOrDefault(i => i.Id == id);
                if (item == null)
                {
                    throw new Exception($"Item with id {id} does not exist");
                }

                return item;
            }
        }

        public Task<IEnumerable<InventoryItem>> GetAll(CancellationToken cancellationToken)
        {
            logger.LogDebug("Getting all items");
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<IEnumerable<InventoryItem>>(cancellationToken);
            }

            return Task.FromResult<IEnumerable<InventoryItem>>(inventoryDB);
        }
    }
}
