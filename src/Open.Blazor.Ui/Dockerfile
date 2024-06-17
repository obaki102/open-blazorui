#docker build -f src/Open.Blazor.Ui/Dockerfile -t blazor-ui .
#docker run -p 8080:80 -d blazor-ui

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Open.Blazor.Ui/Open.Blazor.Ui.csproj", "src/Open.Blazor.Ui/"]
COPY ["src/Open.Blazor.Core/Open.Blazor.Core.csproj", "src/Open.Blazor.Core/"]
RUN dotnet restore "./src/Open.Blazor.Ui/Open.Blazor.Ui.csproj"
COPY . .
WORKDIR "/src/src/Open.Blazor.Ui"
RUN dotnet build "./Open.Blazor.Ui.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Open.Blazor.Ui.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Open.Blazor.Ui.dll"]