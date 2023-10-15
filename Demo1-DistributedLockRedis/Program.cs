using Demo1_DistributedLockRedis;
using Microsoft.Extensions.DependencyInjection;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

Console.WriteLine("Demo1-DistributedLockRedis");

var serviceProvider = new ServiceCollection()
    .AddLogging()
    .AddSingleton(serviceProvider => "file.txt")
    .AddSingleton(serviceProvider =>
    {
        var multiplexers = new List<RedLockMultiplexer>
        {
            ConnectionMultiplexer.Connect("localhost:6379"),
            ConnectionMultiplexer.Connect("localhost:6380"),
            ConnectionMultiplexer.Connect("localhost:6381")
        };
        return RedLockFactory.Create(multiplexers);
    })
    .AddSingleton<FileReaderWriterWithRedLock>()
    .BuildServiceProvider();



var fileReaderWriter = serviceProvider.GetRequiredService<FileReaderWriterWithRedLock>();

var tasks = new List<Task>();

// Create multiple threads to read the file simultaneously
for (int i = 0; i < 10; i++)
{
    tasks.Add(Task.Run(async () =>
    {
        Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} - Reading file...");
        var value = await fileReaderWriter.ReadFileAsync(CancellationToken.None);
        Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} - File content: {value}");
    }));
}

// Wait for all threads to complete
Task.WhenAll(tasks).GetAwaiter().GetResult();

Console.WriteLine("Press any key to exit...");

serviceProvider.Dispose();