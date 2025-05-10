# Copyright 2025 Bradley D. Nelson
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

import base64
from flask import Flask, request, send_from_directory
from google.cloud import datastore

client = datastore.Client()

root_passwd_key = client.key('passwd', 'root')
root_passwd_entity = client.get(root_passwd_key)
if root_passwd_entity is None:
  root_passwd = 'xyzzy'
else:
  root_passwd = root_passwd_entity['secret']

def SaveBlock(index, data):
  assert index >= 0
  assert len(data) == 1024, len(data)
  entity = datastore.Entity(key=client.key('block', index+1))
  entity.update({'data': data})
  client.put(entity)
  return ''

def LoadBlocks(start, end):
  assert end >= end
  assert start >= 0
  assert end - start <= 128
  query = client.query(kind='block')
  first_key = client.key('block', start+1)
  last_key = client.key('block', end+1)
  query.key_filter(first_key, '>=')
  query.key_filter(last_key, '<')
  entities = query.fetch()
  blks = {}
  for i in entities:
    blks[i.key.path[0]['id']-1] = i['data']
  result = []
  for i in range(start, end+1):
    if i in blks:
      result.append(blks[i])
    else:
      result.append(b' ' * 1024)
  return b''.join(result) 

app = Flask(__name__)

@app.route('/<path:filename>')
def canned(filename):
  return send_from_directory('static', filename)

@app.route('/')
def root():
  return canned('index.html')

@app.route('/io', methods=['POST'])
def io():
  if root_passwd != request.form['passwd']:
    return 'deny', 403
  if request.form['command'] == 'read':
    start = int(request.form['start'])
    end = int(request.form['end'])
    return LoadBlocks(start, end)
  elif request.form['command'] == 'write':
    index = int(request.form['index'])
    data = request.files['data'].read()
    return SaveBlock(index, data)

if __name__ == '__main__':
  app.run(host='127.0.0.1', port=8080, debug=True)
