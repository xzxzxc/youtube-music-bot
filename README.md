
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
  sudo apt-get install -y dotnet-sdk-6.0
```

- install python (needed for youtube-dl)
```
sudo apt update
sudo apt install python
python --version
```

- install [youtube-dl](https://github.com/ytdl-org/youtube-dl#installation).
code for linux:
```
sudo curl -L https://yt-dl.org/downloads/latest/youtube-dl -o /usr/local/bin/youtube-dl
sudo chmod a+rx /usr/local/bin/youtube-dl
youtube-dl --version
```
- install ffmpeg
code for linux:
```
sudo add-apt-repository ppa:savoury1/ffmpeg4
sudo apt update
sudo apt install ffmpeg
ffmpeg -version
```
- install mp3splt
```
sudo apt update
sudo apt install mp3splt
mp3splt -version
```

# Installation

- clone this repo. for example to ~/youtube-music-bot folder
```
git clone https://github.com/xzxzxc/youtube-music-bot.git ~/youtube-music-bot
```

- create file named appsettings.Secrets.json in `~/youtube-music-bot/src/Console` folder with bot token.
 
```
{
	"Bot": {
		"Token": "0000000000:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
	}
}
```

# Run

```
cd ~/youtube-music-bot/src/Console/
dotnet run --environment Production
```

# Run with auto-update

- install [git_auto_updater](https://github.com/xzxzxc/git_auto_updater)

- run
```
python git_auto_updater.py -p ~/youtube-music-bot/ -c "dotnet watch run --project ./src/Console --configuration Release -- --environment Production" -g https://github.com/xzxzxc/youtube-music-bot
```

> git v 2.2 or above is required

to update to latest git version
```
sudo add-apt-repository -y ppa:git-core/ppa
sudo apt-get update
sudo apt-get install git -y
```
## Run tests

Unit tests could be run without any additional steps.

To run integration/acceptance tests:
 - Create a [developer account](https://my.telegram.org/) in Telegram.
 - Create file named `Secrets.cs` in `tests/IntegrationTests.Common` with the next structure. _Do not fill `BotUserId` and `BotUserAccessHash`  the first time._
```c#
public static class Secrets
{
  // Token of your test bot (could be asked from @BotFather in telegram) 
  public const string BotToken = "";
  // Id of chat with test bot (could be obtained in debug) 
  public const long UserChatIdForBot = 0;
  // Id of chat where test bot is added as admin (could be obtained in debug) 
  public const long GroupChatIdForBot = 0;
  // Telegram app api_id (could be found here https://my.telegram.org/apps)
  public const int AppApiId = 0;
  // Telegram app api_hash (could be found here https://my.telegram.org/apps)
  public const string AppApiHash = "";
  // Your test user phone number (user must be regestered in telegram)
  public const string UserPhoneNumber = "";
  // Your test bot user_id (could be obtaied if run Console.IntegrationTests as a program)
  public const int BotUserId = 0;
  // Your test bot user_access_hash (could be obtaied if run Console.IntegrationTests as a program)
  public const long BotUserAccessHash = 0;
}
```
 - Authenticate your test user in your test telegram app. For this run:
```
dotnet run --project tests\Console.IntegrationTests
```
 - To obtain `BotUserId` and `BotUserAccessHash` you could use previous command. It asks you to show last user chats. Answer `y` and find your test user data by this user's name (you could use `| grep` for this). If you have no chat with bot - create it manually.
