FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["DiscordNotifier/DiscordNotifier.csproj", "DiscordNotifier/"]
RUN dotnet restore "YogurtTheCommunity/DiscordNotifier.csproj"
COPY . .
WORKDIR "/src/YogurtTheCommunity"
RUN dotnet build "YogurtTheCommunity.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "YogurtTheCommunity.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YogurtTheCommunity.dll"]
