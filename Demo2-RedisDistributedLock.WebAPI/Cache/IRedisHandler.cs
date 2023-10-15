namespace Demo2_RedisDistributedLock.WebAPI.Cache
{
    public interface IRedisHandler : IDisposable
    {
        Task<bool> PerformActionWithLock(string resource, TimeSpan expirationTime, TimeSpan waitTime, TimeSpan retryCount, Func<Task> action);
    }
}
