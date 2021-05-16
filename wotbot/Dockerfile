FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["wotbot.csproj", "./"]
RUN dotnet restore "wotbot.csproj"
COPY . .
WORKDIR "/src/wotbot"
RUN dotnet build "wotbot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "wotbot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "wotbot.dll"]
