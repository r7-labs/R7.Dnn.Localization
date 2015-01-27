#!/bin/bash

# pass args to environment variables
export LPB_PACKVERSION=${1}

# run script
./LanguagePackBuilder.cs

