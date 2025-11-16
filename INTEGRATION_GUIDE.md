# Руководство по интеграции AuthService в другие микросервисы

Это руководство показывает, как использовать AuthService для аутентификации в других микросервисах и создавать профили, привязанные к аккаунтам.

## Архитектура

```
┌─────────────┐
│ AuthService │ ──> Создает Account (email, password)
└──────┬──────┘      Выдает JWT токен с account_id
       │
       │ JWT Token (содержит account_id)
       │
       ▼
┌─────────────┐     ┌──────────────┐     ┌──────────────┐
│UserService │     │WarehouseService│     │DeliveryService│
│            │     │               │     │               │
│ User       │     │ Order         │     │ Delivery     │
│ AccountId  │     │ AccountId     │     │ AccountId    │
└────────────┘     └──────────────┘     └──────────────┘
```

## Варианты использования

### Вариант 1: Создание профилей в других сервисах

После регистрации в AuthService, пользователь может создать профиль в UserService, WarehouseService и т.д., используя `account_id` из JWT токена.

### Вариант 2: Использование в защищенных endpoints

Любой endpoint может требовать аутентификации и использовать `account_id` для работы с данными пользователя.

## Шаг 1: Добавьте зависимость на Auth.Shared

В `.csproj` файле вашего микросервиса:

```xml
<ItemGroup>
  <ProjectReference Include="../../Shared/Auth.Shared/Auth.Shared.csproj" />
</ItemGroup>
```

## Шаг 2: Настройте JWT

### ⚠️ ВАЖНО: Безопасность секретов

**НЕ храните SecretKey в appsettings.json для продакшена!**

Используйте переменные окружения или секретные менеджеры. См. `SECURITY_GUIDE.md` для деталей.

### Для Development (локально):

**Вариант 1: Переменные окружения (рекомендуется)**

```bash
# Создайте .env файл в корне проекта (НЕ КОММИТЬТЕ!)
export JWT_SECRET_KEY=your-super-secret-key-for-auth-service-at-least-32-characters-long-123456789
```

**Вариант 2: appsettings.json (только для разработки)**

```json
{
  "Authentication": {
    "Jwt": {
      "Issuer": "AuthService",
      "Audience": "AuthServiceClients",
      "SecretKey": "dev-key-only-not-for-production"
    }
  }
}
```

**Важно:** 
- SecretKey должен совпадать во всех сервисах
- Для продакшена используйте переменные окружения или секретные менеджеры
- См. `SECURITY_GUIDE.md` для правильной настройки

## Шаг 3: Настройте аутентификацию в Program.cs

```csharp
using Auth.Shared;

var builder = WebApplication.CreateBuilder(args);

// Добавляем JWT аутентификацию
builder.Services.AddJwtAuthentication(builder.Configuration);

// ... остальная конфигурация

var app = builder.Build();

// Включите аутентификацию ПЕРЕД Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
```

## Шаг 4: Добавьте AccountId в ваши сущности

### Пример для UserService

```csharp
// UserService.Domain/Entities/User.cs
public class User : Entity
{
    public Guid AccountId { get; private set; } // Связь с Account из AuthService
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    // ... остальные поля

    public User(Guid accountId, string firstName, string lastName, Phone phone, Email email)
        : base()
    {
        AccountId = accountId;
        FirstName = firstName;
        LastName = lastName;
        // ...
    }
}
```

### Обновите репозиторий

```csharp
public interface IUserRepository
{
    Task<User?> GetByAccountIdAsync(Guid accountId);
    // ... остальные методы
}
```

## Шаг 5: Используйте в контроллерах

### Пример 1: Создание профиля с аутентификацией

```csharp
using Auth.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpPost]
    [Authorize] // Требует JWT токен
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserCommand command)
    {
        // Извлекаем account_id из JWT токена
        var accountId = User.GetAccountId();
        if (!accountId.HasValue)
        {
            return Unauthorized(new { error = "Invalid token" });
        }

        // Проверяем, не создан ли уже профиль
        var existingUser = await _userRepository.GetByAccountIdAsync(accountId.Value);
        if (existingUser != null)
        {
            return Conflict(new { error = "Profile already exists for this account" });
        }

        // Привязываем профиль к account_id
        command.AccountId = accountId.Value;
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(nameof(GetUserById), new { id = result.Value!.Id }, result.Value);
    }
}
```

### Пример 2: Получение своего профиля

```csharp
[HttpGet("me")]
[Authorize]
public async Task<ActionResult<UserDto>> GetMyProfile()
{
    var accountId = User.GetAccountId();
    if (!accountId.HasValue)
    {
        return Unauthorized();
    }

    var user = await _userRepository.GetByAccountIdAsync(accountId.Value);
    if (user == null)
    {
        return NotFound(new { error = "Profile not found. Please create your profile first." });
    }

    return Ok(user.ToDto());
}
```

### Пример 3: Опциональная аутентификация

```csharp
[HttpGet("public-data")]
public IActionResult GetPublicData()
{
    // Если пользователь аутентифицирован, показываем больше данных
    if (User.IsAuthenticatedWithAccount())
    {
        var accountId = User.GetAccountId();
        return Ok(new { 
            data = "Personalized data",
            accountId 
        });
    }

    // Для неаутентифицированных - базовые данные
    return Ok(new { 
        data = "Public data" 
    });
}
```

### Пример 4: Использование в WarehouseService

```csharp
[HttpPost("orders")]
[Authorize]
public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
{
    var accountId = User.GetAccountId();
    if (!accountId.HasValue)
    {
        return Unauthorized();
    }

    // Создаем заказ, привязанный к account_id
    var command = new CreateOrderCommand
    {
        AccountId = accountId.Value,
        Items = dto.Items
    };

    var result = await _mediator.Send(command);
    return Ok(result);
}

[HttpGet("orders/my")]
[Authorize]
public async Task<IActionResult> GetMyOrders()
{
    var accountId = User.GetAccountId();
    if (!accountId.HasValue)
    {
        return Unauthorized();
    }

    var orders = await _orderRepository.GetByAccountIdAsync(accountId.Value);
    return Ok(orders);
}
```

## Шаг 6: Обновите команды и обработчики

```csharp
// UserService.Application/Commands/CreateUserCommand.cs
public class CreateUserCommand : IRequest<Result<UserDto>>
{
    public Guid AccountId { get; set; } // Добавлено
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    // ...
}

// UserService.Application/Commands/CreateUserCommandHandler.cs
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Проверяем, что AccountId не используется
        var exists = await _userRepository.GetByAccountIdAsync(request.AccountId);
        if (exists != null)
        {
            return Result<UserDto>.Failure("User profile already exists for this account");
        }

        var user = new User(
            request.AccountId, // Используем AccountId
            request.FirstName,
            request.LastName,
            new Phone(request.Phone),
            new Email(request.Email)
        );

        await _userRepository.AddAsync(user);
        return Result<UserDto>.Success(user.ToDto());
    }
}
```

## Полный пример: UserService

### 1. Обновите User entity

```csharp
public class User : Entity
{
    public Guid AccountId { get; private set; }
    public string FirstName { get; private set; }
    // ... остальное
}
```

### 2. Обновите репозиторий

```csharp
public interface IUserRepository
{
    Task<User?> GetByAccountIdAsync(Guid accountId);
    // ...
}

public class UserRepository : IUserRepository
{
    public async Task<User?> GetByAccountIdAsync(Guid accountId)
    {
        return await _context.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.AccountId == accountId);
    }
}
```

### 3. Обновите Program.cs

```csharp
using Auth.Shared;

builder.Services.AddJwtAuthentication(builder.Configuration);

// ...

app.UseAuthentication();
app.UseAuthorization();
```

### 4. Обновите контроллер

```csharp
[HttpPost]
[Authorize]
public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto dto)
{
    var accountId = User.GetAccountId();
    if (!accountId.HasValue)
        return Unauthorized();

    var command = new CreateUserCommand
    {
        AccountId = accountId.Value,
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        Phone = dto.Phone,
        Email = dto.Email
    };

    var result = await _mediator.Send(command);
    return Ok(result.Value);
}
```

## Тестирование

### 1. Зарегистрируйтесь в AuthService

```bash
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "password123"
  }'
```

Ответ:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "...",
  "expiresAt": "2024-01-01T12:00:00Z",
  "account": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "email": "user@example.com"
  }
}
```

### 2. Создайте профиль в UserService

```bash
curl -X POST http://localhost:8080/api/users \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "phone": "+79991234567",
    "email": "user@example.com"
  }'
```

### 3. Получите свой профиль

```bash
curl -X GET http://localhost:8080/api/users/me \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

## Важные моменты

1. **SecretKey должен совпадать** во всех сервисах
2. **Issuer и Audience** должны совпадать с AuthService
3. **Всегда проверяйте** наличие `accountId` перед использованием
4. **Один Account = один профиль** в каждом сервисе (или реализуйте логику множественных профилей)
5. **Не храните пароли** в других сервисах - только account_id

## Безопасность

- Используйте HTTPS в продакшене
- Храните SecretKey в переменных окружения или секретах
- Регулярно обновляйте токены
- Реализуйте refresh token механизм для долгоживущих сессий

