# Руководство по безопасности: Хранение секретов

## ❌ Проблема: SecretKey в appsettings.json

**Текущая ситуация (НЕБЕЗОПАСНО для продакшена):**

```json
// appsettings.json - НЕ ДОЛЖЕН содержать реальные секреты!
{
  "Authentication": {
    "Jwt": {
      "SecretKey": "your-super-secret-key-..." // ❌ Плохо!
    }
  }
}
```

### Почему это плохо:

1. **Секреты в Git** - если файл попал в репозиторий, секрет скомпрометирован
2. **Доступ у всех разработчиков** - каждый видит секрет
3. **Сложно ротировать** - нужно менять во всех сервисах
4. **Риск утечки** - при копировании файлов, логировании и т.д.

## ✅ Правильные способы хранения секретов

### Вариант 1: Переменные окружения (рекомендуется для Docker)

#### Для Development (локально):

```bash
# .env файл (добавьте в .gitignore!)
JWT_SECRET_KEY=your-super-secret-key-for-auth-service-at-least-32-characters-long-123456789
```

#### Для Docker Compose:

```yaml
services:
  authservice-api:
    environment:
      Authentication__Jwt__SecretKey: ${JWT_SECRET_KEY}
      Authentication__Jwt__Issuer: AuthService
      Authentication__Jwt__Audience: AuthServiceClients
```

#### В appsettings.json (только для разработки):

```json
{
  "Authentication": {
    "Jwt": {
      "SecretKey": "", // Пустое - будет браться из переменных окружения
      "Issuer": "AuthService",
      "Audience": "AuthServiceClients"
    }
  }
}
```

### Вариант 2: appsettings.Production.json (не коммитить!)

```json
// appsettings.Production.json - НЕ КОММИТИТЬ В GIT!
{
  "Authentication": {
    "Jwt": {
      "SecretKey": "REAL_SECRET_KEY_FROM_SECRET_MANAGER"
    }
  }
}
```

**Важно:** Добавьте в `.gitignore`:
```
appsettings.Production.json
*.secrets.json
.env
```

### Вариант 3: Azure Key Vault / AWS Secrets Manager (для продакшена)

```csharp
// Program.cs
builder.Configuration.AddAzureKeyVault(
    vaultUri: "https://your-vault.vault.azure.net/",
    new DefaultAzureCredential()
);
```

### Вариант 4: Docker Secrets (для Docker Swarm/Kubernetes)

```yaml
secrets:
  jwt_secret_key:
    external: true

services:
  authservice-api:
    secrets:
      - jwt_secret_key
    environment:
      Authentication__Jwt__SecretKey_FILE: /run/secrets/jwt_secret_key
```

## 🔧 Реализация для вашего проекта

### Шаг 1: Обновите docker-compose.yml

```yaml
services:
  authservice-api:
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ASPNETCORE_URLS: http://+:8080
      ConnectionStrings__DefaultConnection: Host=authservice-postgres;Port=5432;Database=AuthServiceDb;Username=postgres;Password=postgres
      Authentication__Jwt__SecretKey: ${JWT_SECRET_KEY:-default-dev-key-change-in-production}
      Authentication__Jwt__Issuer: AuthService
      Authentication__Jwt__Audience: AuthServiceClients
      Authentication__Jwt__ExpirationHours: 24

  userservice-api:
    environment:
      # ... другие настройки
      Authentication__Jwt__SecretKey: ${JWT_SECRET_KEY:-default-dev-key-change-in-production}
      Authentication__Jwt__Issuer: AuthService
      Authentication__Jwt__Audience: AuthServiceClients

  warehouse-service-api:
    environment:
      # ... другие настройки
      Authentication__Jwt__SecretKey: ${JWT_SECRET_KEY:-default-dev-key-change-in-production}
      Authentication__Jwt__Issuer: AuthService
      Authentication__Jwt__Audience: AuthServiceClients
```

### Шаг 2: Создайте .env файл (не коммитить!)

```bash
# .env (в корне проекта)
JWT_SECRET_KEY=your-super-secret-key-for-auth-service-at-least-32-characters-long-123456789
```

### Шаг 3: Обновите .gitignore

```
# Secrets
.env
.env.local
*.secrets.json
appsettings.Production.json
appsettings.*.secrets.json
```

### Шаг 4: Обновите appsettings.json (уберите секреты)

```json
{
  "Authentication": {
    "Jwt": {
      "SecretKey": "", // Будет браться из переменных окружения
      "Issuer": "AuthService",
      "Audience": "AuthServiceClients",
      "ExpirationHours": "24"
    }
  }
}
```

### Шаг 5: Обновите код для fallback

```csharp
// Auth.Shared/JwtAuthenticationExtensions.cs
public static IServiceCollection AddJwtAuthentication(
    this IServiceCollection services,
    IConfiguration configuration)
{
    var secretKey = configuration["Authentication:Jwt:SecretKey"]
        ?? Environment.GetEnvironmentVariable("Authentication__Jwt__SecretKey")
        ?? throw new InvalidOperationException(
            "JWT SecretKey is not configured. " +
            "Set it in appsettings.json or environment variable Authentication__Jwt__SecretKey");
    
    // ...
}
```

## 📋 Чеклист безопасности

### ✅ Для Development:
- [ ] Используйте переменные окружения
- [ ] `.env` файл в `.gitignore`
- [ ] Используйте разные ключи для разных окружений
- [ ] Не коммитьте реальные секреты

### ✅ Для Production:
- [ ] Используйте секретные менеджеры (Azure Key Vault, AWS Secrets Manager)
- [ ] Ротируйте ключи регулярно
- [ ] Используйте разные ключи для разных окружений
- [ ] Ограничьте доступ к секретам
- [ ] Логируйте доступ к секретам
- [ ] Используйте HTTPS везде

## 🚨 Что делать если секрет скомпрометирован

1. **Немедленно** сгенерируйте новый SecretKey
2. Обновите во всех сервисах
3. Инвалидируйте все существующие токены (или дождитесь их истечения)
4. Уведомите пользователей о необходимости перелогиниться
5. Проверьте логи на подозрительную активность

## 📚 Дополнительные ресурсы

- [OWASP Secrets Management](https://cheatsheetseries.owasp.org/cheatsheets/Secrets_Management_Cheat_Sheet.html)
- [12-Factor App: Config](https://12factor.net/config)
- [ASP.NET Core Configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)

