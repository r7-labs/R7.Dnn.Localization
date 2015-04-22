#!/bin/bash

# include package config
source $1/package.config

pushd .
cd ${PACKAGE_DIR}

tx3 pull -l ru_RU

# TODO: fix file names (ru_RU => ru-RU) when packaging?

popd