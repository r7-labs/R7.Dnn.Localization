#!/bin/bash

# include package config
source $1/package.config

pushd .
cd ${PACKAGE_DIR}

tx push --source --skip

popd