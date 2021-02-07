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

vocabulary WiFi   WiFi definitions
transfer{
  WiFi.config
  WiFi.begin WiFi.disconnect
  WiFi.status
  WiFi.macAddress WiFi.localIP
  WiFi.mode
  WiFi.setTxPower WiFi.getTxPower
  WIFI_MODE_APSTA WIFI_MODE_AP WIFI_MODE_STA WIFI_MODE_NULL
}transfer
forth definitions

vocabulary SD_MMC   SD_MMC definitions
transfer{
  SD_MMC.begin SD_MMC.end
  SD_MMC.cardType
  SD_MMC.totalBytes SD_MMC.usedBytes
}transfer
forth definitions

vocabulary SPIFFS   SPIFFS definitions
transfer{
  SPIFFS.begin SPIFFS.end
  SPIFFS.format
  SPIFFS.totalBytes SPIFFS.usedBytes
}transfer
forth definitions

vocabulary ledc  ledc definitions
transfer{
  ledcSetup ledcAttachPin ledcDetachPin
  ledcRead ledcReadFreq
  ledcWrite ledcWriteTone ledcWriteNote
}transfer
forth definitions

vocabulary Serial   Serial definitions
transfer{
  Serial.begin Serial.end
  Serial.available Serial.readBytes
  Serial.write Serial.flush
}transfer
forth definitions

vocabulary bluetooth   bluetooth definitions
?transfer SerialBT.new
?transfer SerialBT.delete
?transfer SerialBT.begin
?transfer SerialBT.end
?transfer SerialBT.available
?transfer SerialBT.readBytes
?transfer SerialBT.write
?transfer SerialBT.flush
?transfer SerialBT.hasClient
?transfer SerialBT.enableSSP
?transfer SerialBT.setPin
?transfer SerialBT.unpairDevice
?transfer SerialBT.connect
?transfer SerialBT.connectAddr
?transfer SerialBT.disconnect
?transfer SerialBT.connected
?transfer SerialBT.isReady
?transfer esp_bt_dev_get_address
forth definitions

internals definitions
transfer{
  malloc sysfree realloc
}transfer
forth definitions

