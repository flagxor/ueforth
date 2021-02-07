( Migrate various words to separate vocabularies )
vocabulary Wire   Wire definitions
transfer{
  Wire.begin Wire.setClock Wire.getClock
  Wire.setTimeout Wire.getTimeout
  Wire.lastError Wire.getErrorText
  Wire.beginTransmission Wire.endTransmission
  Wire.requestFrom Wire.writeTransmission
  Wire.readTransmission Wire.write
  Wire.available Wire.read
  Wire.peek Wire.busy Wire.flush
}transfer
forth definitions

vocabulary WebServer   WebServer definitions
transfer{
  WebServer.arg WebServer.argi WebServer.argName
  WebServer.new WebServer.delete
  WebServer.begin WebServer.stop
  WebServer.on WebServer.hasArg
  WebServer.sendHeader WebServer.send WebServer.sendContent
  WebServer.method WebServer.handleClient
  WebServer.args WebServer.setContentLength
}transfer
forth definitions
