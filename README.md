# NSCPatcher
Network Security Config patcher for Android applications (APKs)

![](https://i.imgur.com/O7k13yp.png)

###### This program is in it's early developing stages so it may not be fully functional and not work for every application. Pull requests are very welcome.

# Features
* Automate the process of patching Network Security Config
    * Decompile
    * Patch
      * Add network_security_config.xml to the XML folder, create the folder if doesn't exist
      * Edit the Manifest to add network_security_config.xml to the application
    * Compile
    * Sign
    * Open the output .apk folder to the user

# Requirements
* Java 8 (JRE 1.8)
* Windows
* SAI installer (If the APKs wont install on your phone)
# Thanks
* [ApkTool](https://github.com/iBotPeaches/Apktool)
* [APK Easy Tool](https://forum.xda-developers.com/t/tool-windows-apk-easy-tool-v1-58-3-dec-2020.3333960/) (Key/cert) (Yes, I am too lazy to make my own)

# Legal
* The developer(s) are not responsible for the use of this program. It should only be used by developers to test their application.
