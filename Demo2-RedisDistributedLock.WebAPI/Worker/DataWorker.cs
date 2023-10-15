namespace Demo2_RedisDistributedLock.WebAPI.Worker
{
    public class DataWorker : IDataWorker
    {

        private readonly string dataFile = "data.txt";
        private readonly ILogger<DataWorker> logger;

        public DataWorker(ILogger<DataWorker> logger)
        {
            this.logger = logger;
        }

        public async Task WriteData(string someText)
        {
            if (!File.Exists(dataFile))
            {
                logger.LogDebug($"Creating file {dataFile}");
                await File.WriteAllTextAsync(dataFile, someText);
            }
            else
            {
                logger.LogDebug($"Appending to file {dataFile}");
                await File.AppendAllTextAsync(dataFile, someText);
            }
        }

        public async Task<string> ReadAll()
        {
            if (File.Exists(dataFile))
            {
                logger.LogDebug($"Reading file {dataFile}");
                return await File.ReadAllTextAsync(dataFile);
            }

            logger.LogDebug($"File {dataFile} does not exist");
            return string.Empty;
        }
    }
}
