FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем ВСЕ csproj
COPY ApiGateway/*.csproj ApiGateway/
COPY Shared/*.csproj Shared/
COPY BackgroundWorker/*.csproj BackgroundWorker/

# Восстанавливаем зависимости ДЛЯ ВСЕГО solution
RUN dotnet restore ApiGateway/ApiGateway.csproj

# Копируем весь код
COPY . .

# Публикуем
RUN dotnet publish ApiGateway/ApiGateway.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
