FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Slate.API/Slate.API.csproj", "Slate.API/"]
COPY ["Slate.Infrastructure/Slate.Infrastructure.csproj", "Slate.Infrastructure/"]
COPY ["Slate.Domain/Slate.Domain.csproj", "Slate.Domain/"]
COPY ["Slate.Application/Slate.Application.csproj", "Slate.Application/"]
RUN dotnet restore "Slate.API/Slate.API.csproj"
COPY . .
WORKDIR "/src/Slate.API"
RUN dotnet build "./Slate.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Slate.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Slate.API.dll"]
