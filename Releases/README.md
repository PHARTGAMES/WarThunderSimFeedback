# WarThunderSimFeedback


ABOUT
=====
War Thunder Plugin for motion telemetry through SimFeedback. (Aircraft only)

https://opensfx.com/

https://github.com/SimFeedback/SimFeedback-AC-Servo


RELEASE NOTES
=============
v1.0 - First pre alpha release.


KNOWN ISSUES WITH THIS PLUGIN 
=============================

1. Telemetry does not work for some aircraft. Only aircraft fitted with an artificial horizon gauge in the cockpit will work.
   The US P-26, that you get at the start DOES NOT have an artificial horizon gauge and will not work with 
   this plugin. If you get no motion and the telemetry light in SimFeedback is red, you can verify that there is no artificial
   horizon gauge by loading a browser and going to http://127.0.0.1:8111/indicators
   The required values are as follows and *if any of them are missing, motion will not work*:
   - aviahorizon_pitch
   - aviahorizon_roll
   - compass
   - speed
   
2. *WARNING* Getting shot down is bad news *WARNING* A shot down scenario will cause your aircraft to spin wildly out of control 
   and in turn your simulator will jump around wildly as a result. 
   I highly suggest using the web controller https://github.com/ffxf/sfb-web-ctrl or some other method to adjust intensity down 
   when you are getting shot down.
   

INSTALLATION INSTRUCTIONS 
=========================

1. Ensure you have the .NET Framework v4.8 runtime installed.

Download: https://dotnet.microsoft.com/download/dotnet-framework/net48

2. Download and extract the latest release zip of WarThunderSimFeedback.

Download: https://github.com/PHARTGAMES/WarThunderSimFeedback/tree/master/Releases

3. Copy the contents of the SimFeedbackPlugin folder within the WarThunderSimFeedback .zip into your SimFeedback root folder.



AUTHOR
======

PEZZALUCIFER


SUPPORT
=======

Support available through SimFeedback owner's discord

https://opensfx.com/simfeedback-setup-and-tuning/#modes