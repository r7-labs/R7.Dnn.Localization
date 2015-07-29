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

tx3 pull -l $CULTURE_CODE --mode=reviewed

popd > /dev/null

# empty entries patch
./RemoveEmptyEntries.cs $PACKAGE_NAME $CULTURE_CODE
