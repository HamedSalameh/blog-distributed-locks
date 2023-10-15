using Microsoft.Extensions.Options;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace Demo2_RedisDistributedLock.WebAPI.Cache
{
    public class RedisHandler : IRedisHandler, IDisposable
    {
        private readonly RedLockFactory redLockFactory;
        private bool disposedValue;
        private readonly IOptions<RedisSettings> redisSettings;
        private readonly ILogger<RedisHandler> _logger;

        public RedisHandler(IOptions<RedisSettings> RedisSettings, ILogger<RedisHandler> logger)
        {
            redisSettings = RedisSettings;
            redLockFactory = CreateRedLockFactory();
            _logger = logger;
        }

        private RedLockFactory CreateRedLockFactory()
        {
            var configurtaion = redisSettings.Value;

            var connectionMultiplexers = new List<RedLockMultiplexer>();
            foreach(var endpoint in configurtaion.RedisEndpoints)
            {
                var connectionMultiplexer = ConnectionMultiplexer.Connect(new ConfigurationOptions
                {
                    EndPoints = { endpoint },               // can be IP:Port or DNS name "redis1.host.com:6379", This is the server endpoint to connect to
                    Password = configurtaion.Password,      // DO NOT USE IN PRODUCTION - only for testing
                    ConnectTimeout = configurtaion.ConnectTimeout,
                    AbortOnConnectFail = false, // don't want failures to sever connection
                    AllowAdmin = true,  // needed for the info command used for lock debugging
                    SyncTimeout = 5000, // milliseconds, default is 5 seconds
                });
                connectionMultiplexers.Add(connectionMultiplexer);
            }
            
            return RedLockFactory.Create(connectionMultiplexers);

        }

        public async Task<bool> PerformActionWithLock(string resource, TimeSpan expirationTime, TimeSpan waitTime, TimeSpan retryTime, Func<Task> action)
        {
            await using (var redLock = await redLockFactory.CreateLockAsync(resource, expirationTime, waitTime, retryTime))
            {
                if (!redLock.IsAcquired)
                {
                    _logger.LogError($"Could not acquire lock for resource {resource}");
                    return false;
                }

                _logger.LogDebug($"Lock acquired for resource {resource}");
                await action();

                return true;

            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _logger.LogDebug("Disposing RedisHandler");
                    redLockFactory?.Dispose();
                }
                disposedValue = true;
            }
        }


        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
