using RedLockNet.SERedis;

namespace Demo1_DistributedLockRedis
{
    public class FileReaderWriterWithRedLock : IDisposable
    {
        // FileReaderWriterWithRedLock - class that reads and writes to a file after acquiring a distributed lock using RedLock.net library
        private readonly string _filePath;
        private readonly RedLockFactory _redLockFactory;

        private TimeSpan defaultExpirationTime = TimeSpan.FromSeconds(5); // 60 seconds
        private TimeSpan defaultWaitTime = TimeSpan.FromSeconds(4); // 10 seconds
        private TimeSpan retryCount = TimeSpan.FromSeconds(3); // 3 times

        private bool disposedValue;

        public FileReaderWriterWithRedLock(string filePath, RedLockFactory redLockFactory)
        {
            _filePath = filePath;
            _redLockFactory = redLockFactory;

            // create the file if it does not exist
            if (!File.Exists(_filePath))
            {
                File.WriteAllTextAsync(_filePath, "BeginContent:");
            }
            else
            {
                File.AppendAllTextAsync(_filePath, GenerateRandomText());
            }
        }

        // method to generate some random text to be saved in a file as lines
        private string GenerateRandomText()
        {
            var random = new Random();
            var randomText = string.Empty;
            for (int i = 0; i < 10; i++)
            {
                randomText += $"Line {i}: {random.Next(1000)}{Environment.NewLine}";
            }
            return randomText;
        }

        // a public method to try to write to the file after acquiring a lock, while keep trying for 30 seconds until the lock is acquired
        public async Task WriteFileAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} - Trying to acquire lock...");
            await using (var redLock = await _redLockFactory.CreateLockAsync("data_file", defaultExpirationTime, defaultWaitTime, retryCount, cancellationToken))
            {
                if (redLock.IsAcquired)
                {
                    Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} - Lock acquired");
                    await File.AppendAllTextAsync(_filePath, GenerateRandomText());
                }
                else
                {
                    Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} - Could not write to file, Lock not acquired. Retrying ...");
                    await WriteFileAsync(cancellationToken);
                }
            }
        }

        // a public method to try to read the file after acquiring a lock, while keep trying for 30 seconds until the lock is acquired
        public async Task<string> ReadFileAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} - Trying to acquire lock...");
            await using (var redLock = await _redLockFactory.CreateLockAsync("data_file", defaultExpirationTime, defaultWaitTime, retryCount, cancellationToken))
            {
                if (redLock.IsAcquired)
                {
                    Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} - Lock acquired");
                    return await File.ReadAllTextAsync(_filePath);
                }
                else
                {
                    Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} - Lock not acquired");
                    return await Task.FromResult(" - Lock not acquired - ");
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} - Disposing...");
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _redLockFactory.Dispose();
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
