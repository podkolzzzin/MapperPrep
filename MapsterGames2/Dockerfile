FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["MapsterGames2/MapsterGames2.csproj", "MapsterGames2/"]
RUN dotnet restore "MapsterGames2/MapsterGames2.csproj"
COPY . .
WORKDIR "/src/MapsterGames2"
RUN dotnet build "MapsterGames2.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MapsterGames2.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MapsterGames2.dll"]
