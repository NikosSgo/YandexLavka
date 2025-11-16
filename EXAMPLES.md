# Примеры использования AuthService в микросервисах

## Пример 1: UserService - Создание профиля с привязкой к Account

### Шаг 1: Добавьте AccountId в User entity

```csharp
// UserService.Domain/Entities/User.cs
public class User : Entity
{
    public Guid AccountId { get; private set; } // Связь с Account
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Phone Phone { get; private set; }
    public Email Email { get; private set; }

    public User(Guid accountId, string firstName, string lastName, Phone phone, Email email)
        : base()
    {
        AccountId = accountId;
        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
        Email = email;
    }
}
```

### Шаг 2: Обновите репозиторий

```csharp
// UserService.Domain/Interfaces/Repositories/IUserRepository.cs
public interface IUserRepository
{
    Task<User?> GetByAccountIdAsync(Guid accountId);
    // ... остальные методы
}

// UserService.Infrastructure/Repositories/UserRepository.cs
public async Task<User?> GetByAccountIdAsync(Guid accountId)
{
    return await _context.Users
        .Include(u => u.Addresses)
        .FirstOrDefaultAsync(u => u.AccountId == accountId);
}
```

### Шаг 3: Обновите команду

```csharp
// UserService.Application/Commands/CreateUserCommand.cs
public class CreateUserCommand : IRequest<Result<UserDto>>
{
    public Guid AccountId { get; set; } // Добавлено
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
```

### Шаг 4: Обновите обработчик

```csharp
// UserService.Application/Commands/CreateUserCommandHandler.cs
public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
{
    // Проверяем, не создан ли уже профиль для этого аккаунта
    var existingUser = await _userRepository.GetByAccountIdAsync(request.AccountId);
    if (existingUser != null)
    {
        return Result<UserDto>.Failure("User profile already exists for this account");
    }

    // Проверяем уникальность телефона и email (если нужно)
    var existsByPhone = await _userRepository.ExistsByPhoneAsync(request.Phone);
    if (existsByPhone)
    {
        return Result<UserDto>.Failure("User with this phone number already exists");
    }

    var phone = new Phone(request.Phone);
    var email = new Email(request.Email);

    var user = new User(request.AccountId, request.FirstName, request.LastName, phone, email);
    await _userRepository.AddAsync(user);

    return Result<UserDto>.Success(user.ToDto());
}
```

### Шаг 5: Обновите контроллер

```csharp
// UserService.API/Controllers/UsersController.cs
using Auth.Shared;
using Microsoft.AspNetCore.Authorization;

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
            return Unauthorized(new { error = "Invalid or missing token" });
        }

        // Привязываем профиль к account_id из токена
        command.AccountId = accountId.Value;

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Error?.Contains("already exists") == true)
            {
                return Conflict(new { error = result.Error });
            }
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(nameof(GetUserById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
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
}
```

### Шаг 6: Настройте Program.cs

```csharp
// UserService.API/Program.cs
using Auth.Shared;

var builder = WebApplication.CreateBuilder(args);

// Добавляем JWT аутентификацию
builder.Services.AddJwtAuthentication(builder.Configuration);

// ... остальная конфигурация

var app = builder.Build();

// ВАЖНО: UseAuthentication должен быть ПЕРЕД UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
```

### Шаг 7: Обновите appsettings.json

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

## Пример 2: WarehouseService - Заказы с привязкой к Account

```csharp
// WareHouse.Domain/Entities/Order.cs
public class Order : Entity
{
    public Guid AccountId { get; private set; } // Связь с Account
    public OrderStatus Status { get; private set; }
    // ... остальные поля

    public Order(Guid accountId, List<OrderItem> items)
    {
        AccountId = accountId;
        Items = items;
        Status = OrderStatus.Created;
    }
}

// WareHouse.API/Controllers/OrdersController.cs
using Auth.Shared;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/orders")]
[Authorize] // Все endpoints требуют аутентификации
public class OrdersController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var accountId = User.GetAccountId();
        if (!accountId.HasValue)
        {
            return Unauthorized();
        }

        var command = new CreateOrderCommand
        {
            AccountId = accountId.Value,
            Items = dto.Items
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders()
    {
        var accountId = User.GetAccountId();
        if (!accountId.HasValue)
        {
            return Unauthorized();
        }

        var query = new GetOrdersByAccountIdQuery { AccountId = accountId.Value };
        var orders = await _mediator.Send(query);
        return Ok(orders);
    }
}
```

## Пример 3: Опциональная аутентификация

```csharp
[HttpGet("products")]
public IActionResult GetProducts()
{
    // Если пользователь аутентифицирован, показываем персональные рекомендации
    if (User.IsAuthenticatedWithAccount())
    {
        var accountId = User.GetAccountId();
        // Получаем персональные рекомендации
        var personalizedProducts = _productService.GetRecommendedForAccount(accountId.Value);
        return Ok(new { 
            products = personalizedProducts,
            personalized = true 
        });
    }

    // Для неаутентифицированных - общий каталог
    var allProducts = _productService.GetAll();
    return Ok(new { 
        products = allProducts,
        personalized = false 
    });
}
```

## Пример 4: Проверка прав доступа

```csharp
[HttpDelete("{id}")]
[Authorize]
public async Task<IActionResult> DeleteOrder(Guid id)
{
    var accountId = User.GetAccountId();
    if (!accountId.HasValue)
    {
        return Unauthorized();
    }

    var order = await _orderRepository.GetByIdAsync(id);
    if (order == null)
    {
        return NotFound();
    }

    // Проверяем, что заказ принадлежит текущему пользователю
    if (order.AccountId != accountId.Value)
    {
        return Forbid("You don't have permission to delete this order");
    }

    await _orderRepository.DeleteAsync(order);
    return NoContent();
}
```

## Тестирование с curl

### 1. Регистрация
```bash
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password123"}'
```

### 2. Вход
```bash
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password123"}'
```

### 3. Создание профиля (с токеном)
```bash
TOKEN="ваш_токен_из_ответа"

curl -X POST http://localhost:8080/api/users \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "phone": "+79991234567",
    "email": "user@example.com"
  }'
```

### 4. Получение своего профиля
```bash
curl -X GET http://localhost:8080/api/users/me \
  -H "Authorization: Bearer $TOKEN"
```

## Важные замечания

1. **Один Account = один профиль** в каждом сервисе (по умолчанию)
2. **AccountId извлекается из токена** - не передавайте его в запросе
3. **Всегда проверяйте** наличие accountId перед использованием
4. **SecretKey должен совпадать** во всех сервисах
5. **Используйте HTTPS** в продакшене

