FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем csproj-файлы
COPY ApiGateway/*.csproj ApiGateway/
COPY Shared/*.csproj Shared/

# Восстанавливаем зависимости
RUN dotnet restore ApiGateway/*.csproj

# Копируем весь код
COPY ApiGateway/ ApiGateway/
COPY Shared/ Shared/

# Публикуем
RUN dotnet publish ApiGateway/*.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
