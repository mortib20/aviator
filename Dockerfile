FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Packages.props", "."]

COPY ["Aviator.Main/Aviator.Main.csproj", "Aviator.Main/"]
COPY ["Aviator.Airframe/Aviator.Airframe.csproj", "Aviator.Airframe/"]
COPY ["Aviator.Adsb/Aviator.Adsb.csproj", "Aviator.Adsb/"]
COPY ["Aviator.Global/Aviator.Global.csproj", "Aviator.Global/"]
COPY ["Aviator.Network/Aviator.Network.csproj", "Aviator.Network/"]

COPY ["Aviator.Main/Aviator.Main.csproj", "Aviator.Main/"]

RUN dotnet restore "Aviator.Main/Aviator.Main.csproj"
COPY . .
WORKDIR "/src/Aviator.Main"
RUN dotnet build "Aviator.Main.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Aviator.Main.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Aviator.Main.dll"]