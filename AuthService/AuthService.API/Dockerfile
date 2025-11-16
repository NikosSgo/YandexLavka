# Используем официальный образ .NET SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем файлы проектов
COPY ["AuthService.API/AuthService.API.csproj", "AuthService.API/"]
COPY ["AuthService.Application/AuthService.Application.csproj", "AuthService.Application/"]
COPY ["AuthService.Domain/AuthService.Domain.csproj", "AuthService.Domain/"]
COPY ["AuthService.Infrastructure/AuthService.Infrastructure.csproj", "AuthService.Infrastructure/"]

# Восстанавливаем зависимости
RUN dotnet restore "AuthService.API/AuthService.API.csproj"

# Копируем весь код
COPY . .

# Собираем проект
WORKDIR "/src/AuthService.API"
RUN dotnet build "AuthService.API.csproj" -c Release -o /app/build

# Публикуем приложение
FROM build AS publish
RUN dotnet publish "AuthService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Финальный образ для запуска
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080

# Копируем опубликованное приложение
COPY --from=publish /app/publish .

# Устанавливаем переменные окружения
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "AuthService.API.dll"]


