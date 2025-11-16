using AuthService.Infrastructure;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Auth Service API",
        Version = "v1",
        Description = "API для регистрации и аутентификации аккаунтов",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Auth Service",
            Email = "support@authservice.com"
        }
    });
});

// Добавляем CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Регистрация Infrastructure и Application слоев
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth Service API V1");
    c.RoutePrefix = string.Empty; // Swagger UI будет доступен на корневом пути
});

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Применяем миграции при запуске
await InitializeDatabaseAsync(app);

app.Run();

static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Ждем, пока база данных станет доступна
        var maxRetries = 30;
        var delay = TimeSpan.FromSeconds(2);
        
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                if (await context.Database.CanConnectAsync())
                {
                    logger.LogInformation("Database connection established.");
                    break;
                }
            }
            catch (Exception)
            {
                if (i == maxRetries - 1)
                {
                    throw;
                }
                
                logger.LogWarning("Waiting for database to be ready... Attempt {Attempt}/{MaxRetries}", i + 1, maxRetries);
                await Task.Delay(delay);
            }
        }
        
        // Создаем базу данных и применяем миграции
        await context.Database.EnsureCreatedAsync();
        logger.LogInformation("Database initialized successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}


