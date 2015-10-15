# unolabs-pi

![Unosquare](http://unosquare.github.io/assets/logo.png)

[Unosquare Labs](http://unosquare.github.io/) C# and Raspberry Pi Demo. Check the [presentation material](https://github.com/unosquare/unolabs-pi/blob/master/documents/Unosquare%20-%20Meetups%20-%20.Net%20On%20Embedded%20Devices%20-%202015-10-14.pptx)

## What you need

* Visual Studio 2015 (Community Edition is just fine)
* Raspberry Pi with a Linux distro and [mono](http://www.mono-project.com/) package.
* Unosquare [sshdeploy](https://github.com/unosquare/sshdeploy) tool 
* Some LEDs and Resistors

## Let's start

1. You need to open the solution with Visual Studio.
2. Edit the ´sshdeploy-run.bat´ file and be sure to specified the correct path to your project, the sshdeploy path and the Raspberry Pi IP.
3. Run the ´sshdeploy-run.bat´ script and be sure the remote connection is successful.
4. Run build the Visual Studio solution :space_invader: and check sshdeploy tool to deploy and run the webapp.
5. You should browse to the Raspberry Pi IP at port 9696, that's all.

**Don't hesitate to send your comments.**
