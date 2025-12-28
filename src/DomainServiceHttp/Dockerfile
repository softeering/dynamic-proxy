# dotnet publish -c Release -r linux-x64 -p:PublishProfile=DefaultContainer -p:ContainerRepository=domainservicehttp
# docker build -t domainservicehttp -f DomainServiceHttp/Dockerfile .

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 7001
EXPOSE 7002

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY [ "DomainGateway.Client.Core/DomainGateway.Client.Core.csproj", "DomainGateway.Client.Core/" ]
COPY [ "DomainGateway.ServiceDiscovery.Client/DomainGateway.ServiceDiscovery.Client.csproj", "DomainGateway.ServiceDiscovery.Client/" ]
COPY [ "DomainServiceHttp/DomainServiceHttp.csproj", "DomainServiceHttp/" ]
RUN dotnet restore "DomainServiceHttp/DomainServiceHttp.csproj"
COPY . .
WORKDIR "/src/DomainServiceHttp"
RUN dotnet build "./DomainServiceHttp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DomainServiceHttp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DomainServiceHttp.dll"]
