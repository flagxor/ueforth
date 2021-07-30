#! /usr/bin/env python
# Copyright 2021 Bradley D. Nelson
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

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
  if name == 'archive.html':
    continue
  sys.stdout.write('<a href="%s">%s</a><br/>\n' % (name, url))
