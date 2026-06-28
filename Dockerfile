FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY FiapGames.Notifications/FiapGames.Notifications.csproj FiapGames.Notifications/
RUN dotnet restore FiapGames.Notifications/FiapGames.Notifications.csproj

COPY FiapGames.Notifications/ FiapGames.Notifications/
RUN dotnet publish FiapGames.Notifications/FiapGames.Notifications.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

EXPOSE 8080
ENTRYPOINT ["dotnet", "FiapGames.Notifications.dll"]
