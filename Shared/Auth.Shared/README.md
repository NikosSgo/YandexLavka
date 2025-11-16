# Auth.Shared

Общая библиотека для JWT аутентификации в микросервисах YandexLavka.

## Использование

### 1. Добавьте зависимость в ваш проект

В `.csproj` файле вашего микросервиса:

```xml
<ItemGroup>
  <ProjectReference Include="../../Shared/Auth.Shared/Auth.Shared.csproj" />
</ItemGroup>
```

### 2. Настройте JWT в appsettings.json

```json
{
  "Authentication": {
    "Jwt": {
      "Issuer": "AuthService",
      "Audience": "AuthServiceClients",
      "SecretKey": "your-super-secret-key-for-auth-service-at-least-32-characters-long-123456789"
    }
  }
}
```

**Важно:** SecretKey должен совпадать с тем, что используется в AuthService!

### 3. Добавьте аутентификацию в Program.cs

```csharp
using Auth.Shared;

var builder = WebApplication.CreateBuilder(args);

// Добавляем JWT аутентификацию
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

// Включите аутентификацию в pipeline
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
```

### 4. Используйте в контроллерах

#### Обязательная аутентификация

```csharp
using Auth.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Требует аутентификации
public class MyController : ControllerBase
{
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        // Извлекаем account_id из токена
        var accountId = User.GetAccountId();
        var email = User.GetEmail();
        
        if (!accountId.HasValue)
        {
            return Unauthorized();
        }

        // Используем accountId для работы с данными
        return Ok(new { accountId, email });
    }
}
```

#### Опциональная аутентификация

```csharp
[HttpGet("public")]
public IActionResult GetPublicData()
{
    // Если пользователь аутентифицирован, используем его account_id
    if (User.IsAuthenticatedWithAccount())
    {
        var accountId = User.GetAccountId();
        // Персонализированные данные
        return Ok(new { personalized = true, accountId });
    }
    
    // Общие данные для неаутентифицированных пользователей
    return Ok(new { personalized = false });
}
```

### 5. Связь профилей с Account

В вашем доменном слое добавьте поле для связи:

```csharp
public class User : Entity
{
    public Guid AccountId { get; private set; } // Связь с Account из AuthService
    public string FirstName { get; private set; }
    // ... остальные поля
}
```

При создании профиля используйте account_id из токена:

```csharp
[HttpPost]
[Authorize]
public async Task<IActionResult> CreateProfile([FromBody] CreateUserCommand command)
{
    var accountId = User.GetAccountId();
    if (!accountId.HasValue)
    {
        return Unauthorized();
    }

    // Проверяем, не создан ли уже профиль для этого аккаунта
    var existingUser = await _userRepository.GetByAccountIdAsync(accountId.Value);
    if (existingUser != null)
    {
        return Conflict("Profile already exists for this account");
    }

    // Создаем профиль с привязкой к account_id
    command.AccountId = accountId.Value;
    var result = await _mediator.Send(command);
    
    return Ok(result);
}
```

## Примеры

### UserService

```csharp
[HttpPost]
[Authorize]
public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
{
    var accountId = User.GetAccountId();
    if (!accountId.HasValue)
        return Unauthorized();

    var command = new CreateUserCommand
    {
        AccountId = accountId.Value,
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        // ...
    };

    var result = await _mediator.Send(command);
    return Ok(result);
}
```

### WarehouseService

```csharp
[HttpPost("orders")]
[Authorize]
public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
{
    var accountId = User.GetAccountId();
    if (!accountId.HasValue)
        return Unauthorized();

    // Создаем заказ, привязанный к account_id
    var order = new Order(accountId.Value, dto.Items);
    await _orderRepository.AddAsync(order);
    
    return Ok(order);
}
```

## Безопасность

- Все защищенные endpoints должны иметь атрибут `[Authorize]`
- Всегда проверяйте наличие `accountId` перед использованием
- Не доверяйте данным из токена без валидации (токен уже валидируется middleware)
- Используйте один и тот же SecretKey во всех сервисах

