#!/bin/bash

if [ ! $1 ]; then
    echo "Error: Specify package name as an argument."
    exit
fi

if [ ! $2 ]; then
    echo "Error: Specify culture 'xx_XX' code as second argument."
    exit
fi

# configs
source common.config
source $PACKAGE_CONFIG

pushd . > /dev/null
cd $PACKAGE_NAME

tx3 pull -l $2 --mode=reviewed

popd > /dev/null