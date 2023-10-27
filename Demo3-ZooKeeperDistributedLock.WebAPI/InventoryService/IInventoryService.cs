using Demo3_ZooKeeperDistributedLock.WebAPI.Controllers;

namespace Demo3_ZooKeeperDistributedLock.WebAPI.InventoryService
{
    // Inventory service interface
    public interface IInventoryService
    {
        Task<InventoryItem> Get(int id, CancellationToken cancellationToken);
        Task<InventoryItem> Add(InventoryItem item, CancellationToken cancellationToken);
        Task<IEnumerable<InventoryItem>> GetAll(CancellationToken cancellationToken);
    }
}
