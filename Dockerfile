# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["src/CareNova.ApiGateway/CareNova.ApiGateway.csproj", "src/CareNova.ApiGateway/"]

RUN dotnet restore "src/CareNova.ApiGateway/CareNova.ApiGateway.csproj"

COPY . .

RUN dotnet publish "src/CareNova.ApiGateway/CareNova.ApiGateway.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "CareNova.ApiGateway.dll"]