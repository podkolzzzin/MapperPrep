FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Mapper.Prep/Mapper.Prep.csproj", "Mapper.Prep/"]
RUN dotnet restore "Mapper.Prep/Mapper.Prep.csproj"
COPY . .
WORKDIR "/src/Mapper.Prep"
RUN dotnet build "Mapper.Prep.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Mapper.Prep.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Mapper.Prep.dll"]
