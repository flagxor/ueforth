#! /usr/bin/env python3
# Copyright 2023 Bradley D. Nelson
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

import os
import subprocess
import sys

base_path = sys.argv[1]
revision_file = sys.argv[2]
revshort_file = sys.argv[3]

revision = subprocess.check_output('git rev-parse HEAD ' + base_path, shell=True).splitlines()[0]
revshort = revision[:7]

if not os.path.exists(revision_file) or open(revision_file).read() != revision:
  with open(revision_file, 'wb') as fh:
    fh.write(revision)
if not os.path.exists(revshort_file) or open(revshort_file).read() != revshort:
  with open(revshort_file, 'wb') as fh:
    fh.write(revshort)
