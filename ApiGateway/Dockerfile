FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем ВСЕ csproj
COPY ApiGateway/*.csproj ApiGateway/
COPY Persistence/*.csproj Persistence/
COPY Contracts/*.csproj Contracts/

# Восстанавливаем зависимости
RUN dotnet restore ApiGateway/ApiGateway.csproj

# Копируем весь код
COPY ApiGateway/ ApiGateway/
COPY Persistence/ Persistence/
COPY Contracts/ Contracts/

# Публикуем
RUN dotnet publish ApiGateway/ApiGateway.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
