# Использование AuthService в других микросервисах

## Два основных варианта использования

### Вариант 1: Создание профилей (рекомендуется)

После регистрации в AuthService пользователь создает профиль в других сервисах, используя `account_id` из JWT токена.

**Пример:**
1. Пользователь регистрируется в AuthService → получает JWT токен
2. Пользователь создает профиль в UserService → профиль привязывается к `account_id`
3. Пользователь создает профиль в WarehouseService → заказы привязываются к `account_id`

**Преимущества:**
- Один аккаунт может использоваться в разных сервисах
- Каждый сервис хранит только свои данные
- Легко масштабировать

### Вариант 2: Использование только в защищенных endpoints

Не создавать профили, а просто использовать `account_id` для идентификации пользователя в операциях.

**Пример:**
- Создание заказа без профиля → заказ привязывается к `account_id`
- Просмотр истории заказов → фильтрация по `account_id`

**Преимущества:**
- Проще реализация
- Не нужно управлять профилями

## Быстрый старт

### 1. Добавьте зависимость

В `.csproj`:
```xml
<ProjectReference Include="../../Shared/Auth.Shared/Auth.Shared.csproj" />
```

### 2. Настройте JWT

В `appsettrings.json`:
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

### 3. Добавьте в Program.cs

```csharp
using Auth.Shared;

builder.Services.AddJwtAuthentication(builder.Configuration);

// ...

app.UseAuthentication();
app.UseAuthorization();
```

### 4. Используйте в контроллерах

```csharp
using Auth.Shared;
using Microsoft.AspNetCore.Authorization;

[Authorize] // Требует токен
public class MyController : ControllerBase
{
    [HttpPost]
    public IActionResult Create([FromBody] CreateDto dto)
    {
        var accountId = User.GetAccountId();
        if (!accountId.HasValue)
            return Unauthorized();

        // Используйте accountId
        return Ok();
    }
}
```

## Примеры использования

### Создание профиля в UserService

```csharp
[HttpPost]
[Authorize]
public async Task<IActionResult> CreateProfile([FromBody] CreateUserDto dto)
{
    var accountId = User.GetAccountId();
    if (!accountId.HasValue)
        return Unauthorized();

    var command = new CreateUserCommand
    {
        AccountId = accountId.Value, // Привязываем к аккаунту
        FirstName = dto.FirstName,
        LastName = dto.LastName
    };

    var result = await _mediator.Send(command);
    return Ok(result);
}
```

### Получение своего профиля

```csharp
[HttpGet("me")]
[Authorize]
public async Task<IActionResult> GetMyProfile()
{
    var accountId = User.GetAccountId();
    if (!accountId.HasValue)
        return Unauthorized();

    var user = await _userRepository.GetByAccountIdAsync(accountId.Value);
    if (user == null)
        return NotFound("Profile not found");

    return Ok(user);
}
```

### Создание заказа с привязкой к Account

```csharp
[HttpPost("orders")]
[Authorize]
public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
{
    var accountId = User.GetAccountId();
    if (!accountId.HasValue)
        return Unauthorized();

    var order = new Order(accountId.Value, dto.Items);
    await _orderRepository.AddAsync(order);
    
    return Ok(order);
}
```

### Опциональная аутентификация

```csharp
[HttpGet("products")]
public IActionResult GetProducts()
{
    if (User.IsAuthenticatedWithAccount())
    {
        // Персональные рекомендации
        var accountId = User.GetAccountId();
        return Ok(_productService.GetRecommended(accountId.Value));
    }
    
    // Общий каталог
    return Ok(_productService.GetAll());
}
```

## Структура данных

### UserService
```csharp
public class User : Entity
{
    public Guid AccountId { get; private set; } // Связь с AuthService
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    // ...
}
```

### WarehouseService
```csharp
public class Order : Entity
{
    public Guid AccountId { get; private set; } // Связь с AuthService
    public OrderStatus Status { get; private set; }
    // ...
}
```

## Полезные методы

### Извлечение данных из токена

```csharp
// Получить account_id
var accountId = User.GetAccountId();

// Получить email
var email = User.GetEmail();

// Проверить аутентификацию
if (User.IsAuthenticatedWithAccount())
{
    // Пользователь аутентифицирован и имеет account_id
}
```

## Безопасность

1. **SecretKey должен совпадать** во всех сервисах
2. **Всегда проверяйте** наличие `accountId` перед использованием
3. **Не передавайте accountId** в запросах - извлекайте из токена
4. **Используйте HTTPS** в продакшене
5. **Храните SecretKey** в переменных окружения или секретах

## Документация

- Полное руководство: `INTEGRATION_GUIDE.md`
- Примеры кода: `EXAMPLES.md`
- Общая библиотека: `Shared/Auth.Shared/README.md`

