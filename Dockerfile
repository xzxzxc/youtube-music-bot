FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build

COPY . youtube-music-bot/

WORKDIR /youtube-music-bot

RUN dotnet restore

RUN for test_proj in ./tests/*; do if echo "$test_proj" | grep -q "UnitTests"; then dotnet test $test_proj; fi || exit 1 ; done || exit 1

RUN dotnet publish ./src/Console -c release -o /publish


FROM mcr.microsoft.com/dotnet/runtime:5.0

WORKDIR /publish

COPY --from=build /publish .

ENTRYPOINT ["dotnet", "Console.dll"]
