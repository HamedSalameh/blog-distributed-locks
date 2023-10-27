using Demo4_ZooKeeperDistributedLock.WebAPI.InventoryService;
using Microsoft.AspNetCore.Mvc;

namespace Demo4_ZooKeeperDistributedLock.WebAPI.Controllers
{
    // Inventory management controller using ZooKeeper distributed lock supports CRUD operations
    [ApiController]
    [Route("[controller]")]
    public class Inventory : Controller
    {
        private readonly IInventoryService inventoryService;

        public Inventory(IInventoryService inventoryService)
        {
            this.inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            return Ok(await inventoryService.GetAll(cancellationToken));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            return Ok(await inventoryService.Get(id, cancellationToken));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] InventoryItem item, CancellationToken cancellationToken)
        {
            return Ok(await inventoryService.Add(item, cancellationToken));
        }
    }
}
