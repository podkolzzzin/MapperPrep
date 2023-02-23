FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["MapsterGames/MapsterGames.csproj", "MapsterGames/"]
RUN dotnet restore "MapsterGames/MapsterGames.csproj"
COPY . .
WORKDIR "/src/MapsterGames"
RUN dotnet build "MapsterGames.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MapsterGames.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MapsterGames.dll"]
