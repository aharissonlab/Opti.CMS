FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore

WORKDIR /src

COPY ["Opti.CMS.csproj", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["nuget.config", "./"]

RUN dotnet restore "./Opti.CMS.csproj" --configfile "./nuget.config"

FROM restore AS publish

COPY . .

RUN dotnet publish "./Opti.CMS.csproj" \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Opti.CMS.dll"]
