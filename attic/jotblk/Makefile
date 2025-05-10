all:
	./run.sh

deploy:
	gcloud app deploy -q --project jotblk *.yaml  

setup:
	python3 -m venv env
	env/bin/pip install -r requirements.txt

datastore:
	echo "Run: . dstore.sh"
