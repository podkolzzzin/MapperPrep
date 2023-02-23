FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["ILGeneration/ILGeneration.csproj", "ILGeneration/"]
RUN dotnet restore "ILGeneration/ILGeneration.csproj"
COPY . .
WORKDIR "/src/ILGeneration"
RUN dotnet build "ILGeneration.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ILGeneration.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ILGeneration.dll"]
