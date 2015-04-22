#!/bin/bash

# include package config
source $1/package.config

# list all source files in the distribution
pushd .
cd ${DISTRO_DIR}

find . -name '*.as?x.resx' -or -name '*Resources.resx' -or -name '*.en-US.resx' | sed 's/\.\///g' > ${PACKAGE_LIST}

popd

# (re)create directories
mkdir ${PACKAGE_DIR}

rm -r -f ${PACKAGE_DIR}/${SOURCE_LANG}
mkdir ${PACKAGE_DIR}/${SOURCE_LANG}

rm -r -f ${PACKAGE_DIR}/.tx
mkdir ${PACKAGE_DIR}/.tx

# copy source files from distribution to package
pushd .
cd ${DISTRO_DIR}

while read FILE; do
    cp -f -r --parents "${FILE}" "../${PACKAGE_DIR}/${SOURCE_LANG}/"
done < ${PACKAGE_LIST}

popd

# create Transifex config
pushd .
cd ${PACKAGE_DIR}

echo "[main]
host = https://www.transifex.com
" > $TX_CONFIG

while read FILE; do
    
    RESOURCE_SLUG=$(echo $FILE | sed 's/\.en-US//g' | sed 's/.resx//g' | sed 's/[ \.\/\-]/_/g' | sed -r 's/_+/_/g' | sed 's/_App_LocalResources//g')
    RESOURCE_FILE_FILTER_BASE=$(echo $FILE | sed 's/\.en-US//g' | sed 's/\.\///g' | sed 's/.resx//g')
    RESOURCE_FILE_FILTER="<lang>/$RESOURCE_FILE_FILTER_BASE.<lang>.resx"
    RESOURCE_SOURCE_FILE="$SOURCE_LANG/$(echo $FILE | sed 's/\.\///g')"
    
    # instead of 'tx set' as it seems to not create file_filter nor trans.<lang> entries
    # tx set -r $PROJECT_SLUG.$RESOURCE_SLUG --source "$RESOURCE_SOURCE_FILE" --language $SOURCE_LANG --type RESX 
    
    echo "[$PROJECT_SLUG.$RESOURCE_SLUG]
file_filter = $RESOURCE_FILE_FILTER
source_file = $RESOURCE_SOURCE_FILE
source_lang = $SOURCE_LANG
type = RESX
" >> $TX_CONFIG

done < ${PACKAGE_LIST}

popd