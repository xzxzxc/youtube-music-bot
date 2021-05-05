
# Youtube music bot

## Telegram bot that downloads audio from youtube/sound-cloud/etc and sends it to You.

> Program is designed for linux but could be run on windows with WSL.

# Pre-requirements

- install [dotnet 5.0 sdk](https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu).
code fot ubuntu 18.04:
```
wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

sudo apt-get update; \
  sudo apt-get install -y apt-transport-https && \
  sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-5.0
```
- install [youtube-dl](https://github.com/ytdl-org/youtube-dl#installation).
code for linux:
```
sudo curl -L https://yt-dl.org/downloads/latest/youtube-dl -o /usr/local/bin/youtube-dl
sudo chmod a+rx /usr/local/bin/youtube-dl
```
- install ffmpeg
code for linux:
```
sudo apt update
sudo apt install ffmpeg
```
- install mp3splt
```
sudo apt update
sudo apt install mp3splt
```
- add nuget package source

```
dotnet nuget add source https://pkgs.dev.azure.com/tgbots/Telegram.Bot/_packaging/Telegram.Bot%40Local/nuget/v3/index.json
```

# Installation

- clone this repo. for example to ~/youtube-music-bot folder
```
git clone https://github.com/xzxzxc/youtube-music-bot.git ~/youtube-music-bot
```

- create appsettings.Secret.json file in `~/youtube-music-bot/src/YoutubeMusicBot` folder with bot token.
 
```
{
	"Bot": {
		"Token": "0000000000:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
	}
}
```

# Run

```
cd ~/youtube-music-bot/src/YoutubeMusicBot/
dotnet run --environment Production
```

# Run with auto-update

- install [git_auto_updater](https://github.com/xzxzxc/git_auto_updater)

```
cd ~/youtube-music-bot/src/YoutubeMusicBot/
dotnet run --environment Production
```

- run
```
python3 git_auto_updater.py -p ~/youtube-music-bot/ -c "dotnet run --project ./src/YoutubeMusicBot/YoutubeMusicBot.csproj --configuration Release -- --environment Production" -g https://github.com/xzxzxc/youtube-music-bot
```

> python 3.6 and git v 2.2 or above is required

to update to latest git version
```
sudo add-apt-repository -y ppa:git-core/ppa
sudo apt-get update
sudo apt-get install git -y
```
