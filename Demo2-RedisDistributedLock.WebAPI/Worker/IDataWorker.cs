namespace Demo2_RedisDistributedLock.WebAPI.Worker
{
    public interface IDataWorker
    {
        Task<string> ReadAll();
        Task WriteData(string someText);
    }
}
