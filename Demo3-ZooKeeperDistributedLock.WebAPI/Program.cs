using Demo3_ZooKeeperDistributedLock.WebAPI.InventoryService;
using Medallion.Threading;
using Medallion.Threading.ZooKeeper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// setup IDistributedLockProvider by madelson/ZooKeeperNetEx
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddSingleton<IDistributedLockProvider>(new ZooKeeperDistributedSynchronizationProvider("localhost:2181",
    options => options.ConnectTimeout(TimeSpan.FromSeconds(5))));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
