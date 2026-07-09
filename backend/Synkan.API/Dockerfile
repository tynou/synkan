FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Synkan.API/Synkan.API.csproj", "Synkan.API/"]
COPY ["Synkan.Infrastructure/Synkan.Infrastructure.csproj", "Synkan.Infrastructure/"]
COPY ["Synkan.Domain/Synkan.Domain.csproj", "Synkan.Domain/"]
COPY ["Synkan.Application/Synkan.Application.csproj", "Synkan.Application/"]
RUN dotnet restore "Synkan.API/Synkan.API.csproj"
COPY . .
WORKDIR "/src/Synkan.API"
RUN dotnet build "./Synkan.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Synkan.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Synkan.API.dll"]
