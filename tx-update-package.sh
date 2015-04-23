#!/bin/bash

if [ ! $1 ]; then
    echo "Error: Specify package name as an argument."
    exit
fi

# configs
source common.config
source $PACKAGE_CONFIG

if [ ! $TX_PROJECT_SLUG ]; then
    echo "Error: Please set TX_PROJECT_SLUG value in the '$PACKAGE_CONFIG' file."
    exit
fi

echo "(Re)create '${PACKAGE_NAME}' package sub-directories."

rm -r -f ${PACKAGE_NAME}/${TX_SOURCE_LANG}
mkdir ${PACKAGE_NAME}/${TX_SOURCE_LANG}

rm -r -f ${PACKAGE_NAME}/.tx
mkdir ${PACKAGE_NAME}/.tx

echo "List all source files in the '$DISTRO_DIR' distribution to the '$SOURCE_FILES' file."
pushd . > /dev/null
cd ${DISTRO_DIR}

find . -name '*.as?x.resx' -or -name '*Resources.resx' -or -name '*.en-US.resx' | sed 's/\.\///g' > "../$SOURCE_FILES"

popd > /dev/null

echo "Copy source files from '$DISTRO_DIR' distribution to the '${PACKAGE_NAME}' package."
pushd . > /dev/null
cd ${DISTRO_DIR}

while read FILE; do
    cp -f -r --parents "${FILE}" "../${PACKAGE_NAME}/${TX_SOURCE_LANG}/"
done < "../${SOURCE_FILES}"

popd > /dev/null

echo "Create Transifex config in '${PACKAGE_NAME}/$TX_CONFIG'..."
pushd . > /dev/null
cd ${PACKAGE_NAME}

echo "[main]
host = https://www.transifex.com
lang_map = $TX_LANG_MAP
" > $TX_CONFIG

while read FILE; do
    
    RESOURCE_SLUG=$(echo $FILE | sed 's/\.en-US//g' | sed 's/.resx//g' | sed 's/[ \.\/\-]/_/g' | sed -r 's/_+/_/g' | sed 's/_App_LocalResources//g')
    RESOURCE_FILE_FILTER_BASE=$(echo $FILE | sed 's/\.en-US//g' | sed 's/\.\///g' | sed 's/.resx//g')
    RESOURCE_FILE_FILTER="<lang>/$RESOURCE_FILE_FILTER_BASE.<lang>.resx"
    RESOURCE_SOURCE_FILE="$TX_SOURCE_LANG/$(echo $FILE | sed 's/\.\///g')"
    
    # instead of 'tx set' as it seems to not create file_filter nor trans.<lang> entries
    # tx set -r $TX_PROJECT_SLUG.$RESOURCE_SLUG --source "$RESOURCE_SOURCE_FILE" --language $TX_SOURCE_LANG --type RESX 
    
    echo "[$TX_PROJECT_SLUG.$RESOURCE_SLUG]
file_filter = $RESOURCE_FILE_FILTER
source_file = $RESOURCE_SOURCE_FILE
source_lang = $TX_SOURCE_LANG
type = RESX
" >> $TX_CONFIG

done < "../$SOURCE_FILES"

echo "Done."

popd > /dev/null
