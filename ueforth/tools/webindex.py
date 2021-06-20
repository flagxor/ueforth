#! /usr/bin/env python

import sys

sys.stdout.write("""<!DOCTYPE html>
<head>
<title>Release Archive</title>
</head>
<body>
<h1>Release Archive</h1>
""")

for line in sys.stdin.read().splitlines():
  url = line.replace('gs://eforth/', 'https://eforth.storage.googleapis.com/')
  name = line.replace('gs://eforth/releases/', '')
  sys.stdout.write('<a href="%s">%s</a><br/>\n' % (name, url))
