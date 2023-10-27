namespace Demo4_ZooKeeperDistributedLock.WebAPI.Controllers
{
    // Inventory item model
    public class InventoryItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int StockCount { get; set; }
    }
}
