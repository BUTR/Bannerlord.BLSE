#!/bin/bash
export DEBIAN_FRONTEND=noninteractive

echo "(*) Upgrading"
apt-get update
apt-get upgrade -y
apt-get clean -y
rm -rf /var/lib/apt/lists/*;

echo "Done!"