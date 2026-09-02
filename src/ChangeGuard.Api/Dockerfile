FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY src/ChangeGuard.Domain/ChangeGuard.Domain.csproj src/ChangeGuard.Domain/
COPY src/ChangeGuard.Application/ChangeGuard.Application.csproj src/ChangeGuard.Application/
COPY src/ChangeGuard.Infrastructure/ChangeGuard.Infrastructure.csproj src/ChangeGuard.Infrastructure/
COPY src/ChangeGuard.Api/ChangeGuard.Api.csproj src/ChangeGuard.Api/
RUN dotnet restore src/ChangeGuard.Api/ChangeGuard.Api.csproj

COPY src/ src/
RUN dotnet publish src/ChangeGuard.Api/ChangeGuard.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "ChangeGuard.Api.dll"]
