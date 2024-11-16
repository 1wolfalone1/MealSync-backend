using MealSync.Application;
using MealSync.Batch;
using MealSync.Batch.BatchLogic;
using MealSync.Batch.Schedulers;
using MealSync.Batch.Services;
using MealSync.Infrastructure;
using Quartz;

DotNetEnv.Env.Load();
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

// Register all IBatchService implementations
var batchServiceType = typeof(IBatchService);
var implementations = AppDomain.CurrentDomain.GetAssemblies()
    .Where(a => a.FullName.StartsWith("MealSync.Batch")) // Filter to your project assemblies
    .SelectMany(s => s.GetTypes())
    .Where(p => batchServiceType.IsAssignableFrom(p) && !p.IsInterface);

// Register each found type in the DI container
foreach (var implementation in implementations)
{
    builder.Services.AddTransient(batchServiceType, implementation);
}

builder.Services.AddTransient<BatchCheckDatabaseService>();
builder.Services.ConfigureInfrastuctureServices(builder.Configuration);
builder.Services.ConfigureApplicationServices(builder.Configuration);
builder.Services.ConfigureQuartzServices(builder.Configuration);

builder.Configuration.AddEnvironmentVariables();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.Run();
