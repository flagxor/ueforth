#! /bin/bash

export DATASTORE_DATASET=jotblk
export DATASTORE_EMULATOR_HOST=127.0.0.1:8099
export DATASTORE_EMULATOR_HOST_PATH=127.0.0.1:8099/datastore
export DATASTORE_HOST=http://127.0.0.1:8099
export DATASTORE_PROJECT_ID=jotblk
env/bin/python3 main.py
