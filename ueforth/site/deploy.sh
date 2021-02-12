#! /bin/bash
export CLOUDSDK_PYTHON=/usr/bin/python
gcloud app deploy --project esp32forth *.yaml
gcloud app deploy --project eforth *.yaml
