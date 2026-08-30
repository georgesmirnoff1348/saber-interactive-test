# Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Скопировать только нужное
COPY . ./

# Восстановить зависимости и собрать
RUN dotnet restore

# Запустить только тесты производительности
CMD ["dotnet", "test"]