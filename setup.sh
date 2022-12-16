#!/bin/bash

set -e # exit on first error

apt-get -qq update
apt-get -y -qq install software-properties-common \
  wget \
  apt-transport-https \
  python3=3.8.2-0ubuntu2 \
  python-is-python3 \
  mp3splt=2.6.2+20170630-3

add-apt-repository -y ppa:savoury1/ffmpeg4

wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

apt-get -qq update

apt-get -y -qq install dotnet-runtime-6.0 ffmpeg=7:4.4.3-0ubuntu1~20.04.sav1

wget https://yt-dl.org/downloads/2021.12.17/youtube-dl -O /usr/local/bin/youtube-dl
chmod a+rx /usr/local/bin/youtube-dl
