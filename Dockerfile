# NuaSpa.Api — glavni REST servis
FROM mcr.microsoft.com/dotnet/sdk:9.0.203-bookworm-slim AS build
WORKDIR /src
COPY . .
RUN dotnet restore "NuaSpa.slnx"
WORKDIR /src/src/NuaSpa.Api
RUN dotnet publish "NuaSpa.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0.3-bookworm-slim AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NuaSpa.Api.dll"]
