FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Debug
ARG ORCHESTRATOR_TCP_PORT=12345
ARG ORCHESTRATOR_UDP_PORT=54321
ARG GITHUB_PACKAGE_TOKEN
ENV GITHUB_TOKEN=${GITHUB_PACKAGE_TOKEN}

WORKDIR /src
RUN dotnet nuget add source https://nuget.pkg.github.com/nakira974/index.json -n github -u nakira974 -p ${GITHUB_PACKAGE_TOKEN} --store-password-in-clear-text

COPY ["Lkhsoft.Dring.Server/Lkhsoft.Dring.Server.csproj", "Lkhsoft.Dring.Server/"]
RUN dotnet restore "Lkhsoft.Dring.Server/Lkhsoft.Dring.Server.csproj"
COPY . .

WORKDIR "/src/Lkhsoft.Dring.Server"
RUN dotnet build "Lkhsoft.Dring.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Lkhsoft.Dring.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE ${ORCHESTRATOR_TCP_PORT} ${ORCHESTRATOR_UDP_PORT}

ENTRYPOINT ["dotnet", "Lkhsoft.Server.dll"]
