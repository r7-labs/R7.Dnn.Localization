#!/bin/bash

if [ ! $1 ]; then
    echo "Error: Specify package name."
    exit
fi

# configs
source common.config
source $PACKAGE_CONFIG

pushd . > /dev/null
cd $PACKAGE_NAME

tx push --source --skip

popd > /dev/null