FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["GeneratorBasedUsage/GeneratorBasedUsage.csproj", "GeneratorBasedUsage/"]
RUN dotnet restore "GeneratorBasedUsage/GeneratorBasedUsage.csproj"
COPY . .
WORKDIR "/src/GeneratorBasedUsage"
RUN dotnet build "GeneratorBasedUsage.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GeneratorBasedUsage.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GeneratorBasedUsage.dll"]
