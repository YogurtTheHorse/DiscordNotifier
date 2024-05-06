FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /YogurtTheCommunity

COPY . ./
RUN dotnet restore
RUN dotnet build -c Release -o build
RUN dotnet publish -c Release -o out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /YogurtTheCommunity
COPY --from=build-env /YogurtTheCommunity/out .
ENTRYPOINT ["dotnet", "YogurtTheCommunity.dll"]
