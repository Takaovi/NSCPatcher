cd NSCPatcher_Data
java -jar "apksigner.jar" sign --key "key.pk8" --cert "cert.pem" --v4-signing-enabled false --out "<apkpath>" "<apkpath>"
exit