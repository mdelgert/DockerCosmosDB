using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure Cosmos DB
var cosmosConnectionString = builder.Configuration["CosmosDb:ConnectionString"];
var cosmosDatabaseName = builder.Configuration["CosmosDb:DatabaseName"];
var cosmosContainerName = builder.Configuration["CosmosDb:ContainerName"];

// Add DisableServerCertificateValidation for local emulator
if (builder.Environment.IsDevelopment())
{
    cosmosConnectionString += "DisableServerCertificateValidation=True;";
}

// Register CosmosClient as singleton (best practice)
builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
{
    return new CosmosClient(cosmosConnectionString);
});

// Register Container as singleton with lazy initialization
builder.Services.AddSingleton<Container>(serviceProvider =>
{
    var cosmosClient = serviceProvider.GetRequiredService<CosmosClient>();
    
    // Get database and container references (will be created on first use)
    var database = cosmosClient.GetDatabase(cosmosDatabaseName);
    var container = database.GetContainer(cosmosContainerName);
    
    return container;
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Initialize Cosmos DB database and container on startup (best practice for production)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var cosmosClient = scope.ServiceProvider.GetRequiredService<CosmosClient>();
        
        // Create database if it doesn't exist
        var databaseResponse = await cosmosClient.CreateDatabaseIfNotExistsAsync(cosmosDatabaseName);
        
        // Create container if it doesn't exist
        var containerProperties = new ContainerProperties(cosmosContainerName, "/id");
        await databaseResponse.Database.CreateContainerIfNotExistsAsync(containerProperties);
        
        app.Logger.LogInformation("Cosmos DB database and container initialized successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Failed to initialize Cosmos DB");
        // Don't throw - allow app to start anyway for development scenarios
    }
}

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
