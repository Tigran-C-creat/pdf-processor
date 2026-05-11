FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем csproj-файлы
COPY BackgroundWorker/*.csproj BackgroundWorker/
COPY Persistence/*.csproj Persistence/
COPY Contracts/*.csproj Contracts/

# Восстанавливаем зависимости
RUN dotnet restore BackgroundWorker/BackgroundWorker.csproj

# Копируем весь код
COPY BackgroundWorker/ BackgroundWorker/
COPY Persistence/ Persistence/
COPY Contracts/ Contracts/

# Публикуем
RUN dotnet publish BackgroundWorker/BackgroundWorker.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BackgroundWorker.dll"]
