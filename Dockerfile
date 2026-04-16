# 1. Stage: Build (Koristimo SDK 9.0)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Kopiramo cijeli src folder jer Api zavisi od ostalih projekata (Domain, App...)
COPY . .

# Radimo restore za cijeli solution da pohvata sve zavisnosti
RUN dotnet restore "NuaSpa.slnx"

# Buildamo konkretno API projekt
WORKDIR "/src/src/NuaSpa.Api"
RUN dotnet build "NuaSpa.Api.csproj" -c Release -o /app/build

# 2. Stage: Publish
FROM build AS publish
RUN dotnet publish "NuaSpa.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 3. Stage: Final (Runtime koristi ASP.NET 9.0)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NuaSpa.Api.dll"]