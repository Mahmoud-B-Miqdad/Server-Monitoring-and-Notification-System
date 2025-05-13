# استخدم aspnet بدلاً من runtime لدعم SignalR
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ServerMonitoringSystem.ConsumerClient/ServerMonitoringSystem.ConsumerClient.csproj", "ServerMonitoringSystem.ConsumerClient/"]
RUN dotnet restore "./ServerMonitoringSystem.ConsumerClient/ServerMonitoringSystem.ConsumerClient.csproj"
COPY . .
WORKDIR "/src/ServerMonitoringSystem.ConsumerClient"
RUN dotnet build "./ServerMonitoringSystem.ConsumerClient.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ServerMonitoringSystem.ConsumerClient.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ServerMonitoringSystem.ConsumerClient.dll"]
