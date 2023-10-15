using Demo2_RedisDistributedLock.WebAPI.Cache;
using Demo2_RedisDistributedLock.WebAPI.Worker;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Demo2_RedisDistributedLock.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : Controller
    {
        private readonly IRedisHandler _redisHandler;
        private readonly ILogger<HomeController> _logger;
        private readonly IDataWorker _dataWorker;

        public HomeController(IRedisHandler redisHandler, IDataWorker dataWorker, ILogger<HomeController> logger)
        {
            _redisHandler = redisHandler ?? throw new ArgumentNullException(nameof(redisHandler));
            _dataWorker = dataWorker ?? throw new ArgumentNullException(nameof(dataWorker));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> ReadAllData()
        {
            var dataContent = string.Empty;
            
            _logger.LogDebug("Reading all data request");

            var result = await _redisHandler
                .PerformActionWithLock("ReadAllData", TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10), async () =>
            {
                dataContent = await readAllData();
            });

            if (!result)
            {
                return StatusCode(500);
            }

            return Ok(dataContent);
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> WriteData([FromBody] string data)
        {
            _logger.LogDebug("Writing data request");

            var result = await _redisHandler
                .PerformActionWithLock("WriteData", TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10), async () =>
                {
                await _dataWorker.WriteData(data);
            });

            if (!result)
            {
                return StatusCode(500);
            }

            return Ok();
        }

        private async Task<string> readAllData()
        {
            return await _dataWorker.ReadAll();
        }
    }
}
