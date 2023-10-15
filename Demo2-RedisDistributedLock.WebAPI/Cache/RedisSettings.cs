namespace Demo2_RedisDistributedLock.WebAPI.Cache
{
    public class RedisSettings
    {
        public string[] RedisEndpoints { get; set; }
        public string InstanceName { get; set; }
        public string Password { get; set; }
        public int MaxRetries { get; set; }
        public TimeSpan MinDelayBetweenRetries { get; set; }
        public int Database { get; set; }
        public int ConnectTimeout { get; set; } = 5000;
    }
}
