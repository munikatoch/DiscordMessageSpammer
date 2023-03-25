#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DiscordPokemonNameBot/DiscordPokemonNameBot.csproj", "DiscordPokemonNameBot/"]
COPY ["PokemonPredictor/PokemonPredictor.csproj", "PokemonPredictor/"]
COPY ["AppLogger/Logging.csproj", "AppLogger/"]
COPY ["Common/Common.csproj", "Common/"]
COPY ["Models/Models.csproj", "Models/"]
COPY ["Interfaces/Interfaces.csproj", "Interfaces/"]
RUN dotnet restore "DiscordPokemonNameBot/DiscordPokemonNameBot.csproj"
COPY . .
WORKDIR "/src/DiscordPokemonNameBot"
RUN dotnet build "DiscordPokemonNameBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DiscordPokemonNameBot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DiscordPokemonNameBot.dll"]