FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG VERSION=0.0.0-dev
WORKDIR /src
COPY Directory.Build.props ./
COPY src/MythByte.Domain/MythByte.Domain.csproj src/MythByte.Domain/
COPY src/MythByte.Shared/MythByte.Shared.csproj src/MythByte.Shared/
COPY src/MythByte.Application/MythByte.Application.csproj src/MythByte.Application/
COPY src/MythByte.Infrastructure/MythByte.Infrastructure.csproj src/MythByte.Infrastructure/
COPY src/MythByte.Networking/MythByte.Networking.csproj src/MythByte.Networking/
COPY src/MythByte.GameServer/MythByte.GameServer.csproj src/MythByte.GameServer/
RUN dotnet restore src/MythByte.GameServer/MythByte.GameServer.csproj
COPY src/ src/
RUN dotnet publish src/MythByte.GameServer/MythByte.GameServer.csproj -c Release -o /app/publish --no-restore -p:Version=${VERSION}

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
EXPOSE 7780/udp
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "MythByte.GameServer.dll"]
