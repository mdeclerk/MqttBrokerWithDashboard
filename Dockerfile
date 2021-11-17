# Build environment
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /src
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /publish --nologo

# Runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime-env
WORKDIR /publish
COPY --from=build-env /publish .
ENTRYPOINT dotnet MqttBrokerWithDashboard.dll