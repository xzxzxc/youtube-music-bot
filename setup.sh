#!/bin/sh

# TODO: add versions
add-apt-repository ppa:savoury1/ffmpeg4
apt update
apt --assume-yes install wget curl apt-transport-https python mp3splt

wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

apt update

apt --assume-yes install -y dotnet-runtime-6.0 ffmpeg

curl -L https://yt-dl.org/downloads/latest/youtube-dl -o /usr/local/bin/youtube-dl
chmod a+rx /usr/local/bin/youtube-dl
