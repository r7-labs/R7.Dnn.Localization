#!/bin/bash

LOCAL_DIR="$HOME/Projects/R7.DnnLocalization"
FTP_HOST="www.somesite.com"
FTP_DIR=""
FTP_USER="$USER"
FTP_PASSWD="password"
WGET_EXCLUDED_DIRS="$FTP_DIR/Portals/0,$FTP_DIR/Portals/1,$FTP_DIR/Portals/2,$FTP_DIR/Portals/3,$FTP_DIR/bin,$FTP_DIR/App_Code,$FTP_DIR/App_Data,$FTP_DIR/App_Browsers,$FTP_DIR/Install/Temp,$FTP_DIR/js"
WGET_CUT_DIRS=0 # set to FTP_DIR depth
RESX_LOCALE="ru-RU"

# create target dir
cd "$LOCAL_DIR"
mkdir -p "$RESX_LOCALE"
cd "$RESX_LOCALE"

# get resource files for given locale from FTP site
wget -N -nH -r -l inf -A $RESX_LOCALE.resx -X$WGET_EXCLUDED_DIRS --cut-dirs=$WGET_CUT_DIRS --ignore-case ftp://$FTP_USER:$FTP_PASSWD@$FTP_HOST$FTP_DIR

# delete empty dirs
find . -type d -empty -delete

