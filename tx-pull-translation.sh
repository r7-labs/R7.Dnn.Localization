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

tx3 pull -l ru_RU

popd > /dev/null