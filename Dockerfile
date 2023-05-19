FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["PackageVersionAction/PackageVersionAction.csproj", "PackageVersionAction/"]
RUN dotnet restore "PackageVersionAction/PackageVersionAction.csproj"
COPY . .
WORKDIR "/src/PackageVersionAction"
RUN dotnet build "PackageVersionAction.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PackageVersionAction.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PackageVersionAction.dll"]
