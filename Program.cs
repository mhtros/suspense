using Suspense.Server.Configurations.Collection.Extensions;
using Suspense.Server.Hubs;
using Suspense.Server.Repository;
using Suspense.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger(1, 0);

builder.Services.AddSignalR(
    // Uncomment only for debugging purposes 
    // config =>
    // {
    //     config.KeepAliveInterval = TimeSpan.FromHours(1);
    //     config.ClientTimeoutInterval = TimeSpan.FromHours(1);
    //     config.EnableDetailedErrors = true;
    // }
);

builder.Services.AddTransient<IGameRepository, GameRepository>();
builder.Services.AddTransient<IPlayerRepository, PlayerRepository>();
builder.Services.AddSingleton<IGameManagerFactory, GameManagerFactory>();
builder.Services.AddSingleton<IBotMoveCalculator, BotMoveCalculator>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    var host = Environment.GetEnvironmentVariable("REDIS_HOST");
    var password = Environment.GetEnvironmentVariable("REDIS_PASSWORD");

    options.Configuration = builder.Environment.IsProduction() ? $"{host},password={password}" : "localhost:6379";
    options.InstanceName = "suspense.";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHub<GameHub>("/hubs/game");
app.UseHttpsRedirection();
app.MapControllers();

if (app.Environment.IsProduction())
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}
else
{
    app.UseCors(x => x
        .WithOrigins("http://localhost:3000")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
}

app.Run();