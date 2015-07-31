#!/bin/bash

if [ ! $1 ]; then
    echo "Error: Specify package name as first argument."
    exit
fi

if [ ! $2 ]; then
    echo "Error: Specify culture code 'xx-XX' or 'xx_XX' as the second argument."
    exit
fi

source common.config
source $PACKAGE_CONFIG

CULTURE_CODE=$2

# pass args to environment variables
export LPB_PACKAGE_TYPE="$PACKAGE_TYPE"
export LPB_PACKAGE_VERSION="$PACKAGE_VERSION"
export LPB_SOURCE_VERSION="$SOURCE_VERSION"
export LBP_PLATFORM_TYPE="$PLATFORM_TYPE"
export LBP_EXTENSION_ASSEMBLY="$EXTENSION_ASSEMBLY"

# run script
./LanguagePackBuilder.cs $PACKAGE_NAME $CULTURE_CODE
