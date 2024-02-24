#! /bin/bash

set -e

echo "Upgrading"
sudo apt-get update && sudo apt-get upgrade -y

echo "Installing dependencies"
sudo apt-get install -y \
  build-essential \
  cmake \
  gcc-arm-none-eabi \
  ninja-build \
  nodejs
