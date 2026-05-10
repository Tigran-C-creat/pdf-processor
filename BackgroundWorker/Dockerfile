FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем csproj-файлы
COPY BackgroundWorker/*.csproj BackgroundWorker/
COPY Shared/*.csproj Shared/

# Восстанавливаем зависимости
RUN dotnet restore BackgroundWorker/*.csproj

# Копируем весь код
COPY BackgroundWorker/ BackgroundWorker/
COPY Shared/ Shared/

# Публикуем
RUN dotnet publish BackgroundWorker/*.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BackgroundWorker.dll"]
