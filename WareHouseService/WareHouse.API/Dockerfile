FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["WareHouse.API/WareHouse.API.csproj", "WareHouse.API/"]
COPY ["WareHouse.Application/WareHouse.Application.csproj", "WareHouse.Application/"]
COPY ["WareHouse.Domain/WareHouse.Domain.csproj", "WareHouse.Domain/"]
COPY ["WareHouse.Infrastructure/WareHouse.Infrastructure.csproj", "WareHouse.Infrastructure/"]

RUN dotnet restore "WareHouse.API/WareHouse.API.csproj"

COPY . .

WORKDIR "/src/WareHouse.API"
RUN dotnet build "WareHouse.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WareHouse.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "WareHouse.API.dll"]
