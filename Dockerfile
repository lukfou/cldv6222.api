# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["CLDV6212POE1.csproj", "."]
RUN dotnet restore "CLDV6212POE1.csproj"

COPY . .

RUN dotnet publish "CLDV6212POE1.csproj" -c Release -o /app/publish /p:UseAppHost=false


# Runtime stage
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0

WORKDIR /home/site/wwwroot

COPY --from=build /app/publish .

EXPOSE 80