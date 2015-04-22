#!/bin/bash

# make a copy of tx script
sudo rm -f "/usr/local/bin/tx3"
sudo cp -f "/usr/local/bin/tx" "/usr/local/bin/tx3"

# patch tx3 to use Python 3 (need for tx pull work)
sudo sed -i "s|#!/usr/bin/python|#!/usr/bin/python3|" "/usr/local/bin/tx3"