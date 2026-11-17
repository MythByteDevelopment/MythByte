FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG VERSION=0.0.0-dev
WORKDIR /src
COPY Directory.Build.props ./
COPY src/MythByte.Domain/MythByte.Domain.csproj src/MythByte.Domain/
COPY src/MythByte.Shared/MythByte.Shared.csproj src/MythByte.Shared/
COPY src/MythByte.Application/MythByte.Application.csproj src/MythByte.Application/
COPY src/MythByte.Infrastructure/MythByte.Infrastructure.csproj src/MythByte.Infrastructure/
COPY src/MythByte.API/MythByte.API.csproj src/MythByte.API/
RUN dotnet restore src/MythByte.API/MythByte.API.csproj
COPY src/ src/
RUN dotnet publish src/MythByte.API/MythByte.API.csproj -c Release -o /app/publish --no-restore -p:Version=${VERSION}

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "MythByte.API.dll"]
