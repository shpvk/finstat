FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/Finstat.Domain/*.csproj src/Finstat.Domain/
COPY src/Finstat.Application/*.csproj src/Finstat.Application/
COPY src/Finstat.Infrastructure/*.csproj src/Finstat.Infrastructure/
COPY src/Finstat.Api/*.csproj src/Finstat.Api/
RUN dotnet restore src/Finstat.Api
COPY src src
RUN dotnet publish src/Finstat.Api -c Release --no-restore -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /out .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Finstat.Api.dll"]
