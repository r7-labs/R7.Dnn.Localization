#!/bin/bash

if [ ! $1 ]; then
    echo "Error: Specify package name as an argument."
    exit
fi

if [ ! $2 ]; then
    echo "Error: Specify culture code 'xx-XX' or 'xx_XX' as the second argument."
    exit
fi

# configs
source common.config
source $PACKAGE_CONFIG

CULTURE_CODE=${2/-/_}

pushd . > /dev/null
cd $PACKAGE_NAME

shift 2
tx pull -l $CULTURE_CODE "$@"
PULL_ERROR=$?

popd > /dev/null

# empty entries patch
if [ $PULL_ERROR -eq 0 ]; then
    ./RemoveEmptyEntries.cs $PACKAGE_NAME $CULTURE_CODE
fi
