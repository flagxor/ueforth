// Copyright 2024 Bradley D. Nelson
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

/*
 * ESP32forth RMT v{{VERSION}}
 * Revision: {{REVISION}}
 */

#include "HTTPClient.h"

#define OPTIONAL_HTTP_CLIENT_VOCABULARY V(HTTPClient)
#define OPTIONAL_HTTP_CLIENT_SUPPORT \
  XV(HTTPClient, "NetworkClientSecure.new", NetworkClientSecure_new, PUSH new NetworkClientSecure()) \
  XV(HTTPClient, "NetworkClientSecure.delete", NetworkClientSecure_delete, delete ((NetworkClientSecure *) a0); DROP) \
  XV(HTTPClient, "NetworkClientSecure.setCACert", NetworkClientSecure_setCACert, \
      ((NetworkClientSecure *) a0)->setCACert(c1); DROPn(2)) \
  XV(HTTPClient, "HTTPClient.new", HTTPClient_new, PUSH new HTTPClient()) \
  XV(HTTPClient, "HTTPClient.delete", HTTPClient_delete, delete (HTTPClient *) a0; DROP) \
  XV(HTTPClient, "HTTPClient.begin", HTTPClient_begin, n0 = ((HTTPClient *) a0)->begin(c1); NIP) \
  XV(HTTPClient, "HTTPClient.beginNC", HTTPClient_beginNC, \
      n0 = ((HTTPClient *) a0)->begin(*(NetworkClient *)a1, c2); NIPn(2)) \
  XV(HTTPClient, "HTTPClient.end", HTTPClient_end, ((HTTPClient *) a0)->end(); DROP) \
  XV(HTTPClient, "HTTPClient.connected", HTTPClient_connected, n0 = ((HTTPClient *) a0)->connected()) \
  XV(HTTPClient, "HTTPClient.setReuse", HTTPClient_setReuse, ((HTTPClient *) a0)->setReuse(n1); DROPn(2)) \
  XV(HTTPClient, "HTTPClient.setUserAgent", HTTPClient_setUserAgent, ((HTTPClient *) a0)->setUserAgent(c1); DROPn(2)) \
  XV(HTTPClient, "HTTPClient.setAuthorization", HTTPClient_setAuthorization, \
      ((HTTPClient *) a0)->setAuthorization(c2, cc1); DROPn(3)) \
  XV(HTTPClient, "HTTPClient.setFollowRedirects", HTTPClient_setFollowRedirects, \
      ((HTTPClient *) a0)->setFollowRedirects((followRedirects_t) n1); DROPn(2)) \
  XV(HTTPClient, "HTTPClient.setRedirectLimit", HTTPClient_setRedirectLimit, \
      ((HTTPClient *) a0)->setRedirectLimit(n1); DROPn(2)) \
  XV(HTTPClient, "HTTPClient.GET", HTTPClient_GET, n0 = ((HTTPClient *) a0)->GET()) \
  XV(HTTPClient, "HTTPClient.POST", HTTPClient_POST, n0 = ((HTTPClient *) a0)->POST(b2, n1); NIPn(2)) \
  XV(HTTPClient, "HTTPClient.addHeader", HTTPClient_addHeader, ((HTTPClient *) a0)->addHeader(c2, c1); DROPn(3)) \
  XV(HTTPClient, "HTTPClient.sendRequest", HTTPClient_sendRequest, n0 = ((HTTPClient *) a0)->sendRequest(c3, b2, n1); NIPn(3)) \
  XV(HTTPClient, "HTTPClient.getSize", HTTPClient_getSize, n0 = ((HTTPClient *) a0)->getSize()) \
  XV(HTTPClient, "HTTPClient.getString", HTTPClient_getString, ((HTTPClient *) a0)->getString().getBytes(b2, n1); DROPn(3)) \
  XV(HTTPClient, "HTTPClient.getStreamPtr", HTTPClient_getStreamPtr, \
      NetworkClient *s = ((HTTPClient *) a0)->getStreamPtr(); n0 = (cell_t) s) \
  XV(HTTPClient, "NetworkClient.available", NetworkClient_available, n0 = ((NetworkClient *) a0)->available()) \
  XV(HTTPClient, "NetworkClient.readBytes", NetworkClient_readBytes, n0 = ((NetworkClient *) a0)->readBytes(b2, n1); NIPn(2))
