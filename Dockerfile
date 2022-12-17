FROM ubuntu:focal AS run

COPY ./setup.sh /setup.sh
RUN chmod +x /setup.sh && /setup.sh

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

COPY src/ youtube-music-bot/src
COPY youtube-music-bot.sln youtube-music-bot/
WORKDIR /youtube-music-bot

RUN dotnet restore && dotnet publish ./src/Console -c release -o /publish

FROM run

COPY --from=build /publish /publish
WORKDIR /publish

ENTRYPOINT ["dotnet", "Console.dll"]
