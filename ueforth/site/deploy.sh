#! /bin/bash
export CLOUDSDK_PYTHON=/usr/bin/python
gcloud app deploy --project eforth *.yaml
