using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Http.Json;

namespace Demo3_ZooKeeperDistributedLock.Tester
{
    public class InventoryItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int StockCount { get; set; }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            var item = new InventoryItem
            {
                Id = 1,
                Name = "Item 1",
                Description = "Item 1 description",
                StockCount = 10
            };

            var httpClient = new HttpClient();
            var response = httpClient.PostAsJsonAsync("https://localhost:7007/inventory", item).Result;

            Console.WriteLine(response.StatusCode);
            Console.WriteLine(response.Content);

            // send 100 concurrent requests to get inventory item with id 10
            var tasks = new List<Task>();
            for (int i = 0; i < 100; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var httpClient = new HttpClient();
                    var response = httpClient.GetAsync("https://localhost:7007/inventory/1").Result;
                    Console.WriteLine(response.StatusCode);
                    Console.WriteLine(response.Content);
                }));
            }
            Console.WriteLine(
                $"Total {tasks.Count} tasks are created, waiting for all tasks to complete...");
            Task.WaitAll(tasks.ToArray());
            Console.WriteLine("All tasks are completed");

        }
    }
}