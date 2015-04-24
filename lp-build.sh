#!/bin/bash

if [ ! $1 ]; then
    echo "Error: Specify package name as first argument."
    exit
fi

if [ ! $2 ]; then
    echo "Error: Specify culture 'xx-XX' code as second argument."
    exit
fi

# declare native culture names
declare -A CULTURE_NAME_NATIVE
CULTURE_NAME_NATIVE["en-US"]="English (United States)"
CULTURE_NAME_NATIVE["ru-RU"]="Русский (Россия)"

source common.config
source $PACKAGE_CONFIG

# pass args to environment variables
export LPB_CULTURE_CODE="$2"
export LPB_CULTURE_NAME_NATIVE="${CULTURE_NAME_NATIVE[$2]}"
export LPB_PACKAGE_TYPE="$PACKAGE_TYPE"
export LPB_PACKAGE_NAME="$PACKAGE_NAME"
export LPB_PACKAGE_VERSION="$PACKAGE_VERSION"
export LPB_SOURCE_VERSION="$SOURCE_VERSION"
export LBP_PLATFORM_TYPE="$PLATFORM_TYPE"

# run script
./LanguagePackBuilder.cs

