
#Instalation

- install dotnet 5.0 sdk
```
wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

sudo apt-get update; \
  sudo apt-get install -y apt-transport-https && \
  sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-5.0
```
- add package
dotnet nuget add source https://pkgs.dev.azure.com/tgbots/Telegram.Bot/_packaging/Telegram.Bot%40Local/nuget/v3/index.json
- clone this repo
- create file with bot token

