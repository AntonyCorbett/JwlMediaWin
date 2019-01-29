# JwlMediaWin <img src="https://ci.appveyor.com/api/projects/status/iflm7hmfrl4ffqjw/branch/master?svg=true">

TrayIcon utility to modify JW Library media window.

![Main Window](http://cv8.org.uk/soundbox/JwlMediaWin/Images/JwlMediaWin.png)

Some find the JW Library media window a little difficult to manage when using 3rd-party applications such as
VLC and OnlyM to display images or videos that aren't included in JW Library. The media window is sometimes lost 
during transition between applications and occasionally the year text disappears.

JwlMediaWin changes the behaviour of the JW Library media window so that it is not minimized whenever another 
application opens a window on the media display. The JW Library application code is not modified, rather 
UI Automation is used to transform the media window (i.e. it performs functions you _could_ do manually).

### Download

If you just want to install the application, please download the [JwlMediaWinSetup.exe](https://github.com/AntonyCorbett/JwlMediaWin/releases/latest) file.

### Help

See the [wiki](https://github.com/AntonyCorbett/JwlMediaWin/wiki) for basic instructions and for information on where to get further help.

### License, etc

JwlMediaWin is Copyright &copy; 2019 Antony Corbett and other contributors under the [MIT license](LICENSE). NotifyIconWpf (Philipp Sumi) is used under the Code Project Open License (CPOL) 1.02. InputSimulator (Michael Noonan, Theodoros Chatzigiannakis) is used under Microsoft Public License (MS-PL). Newtonsoft.Json (James Newton-King) is used under MIT. UIAComWrapperX (Techno Scavenger) is used unde MIT. GalaSoft MVVM Light (Laurent Bugnion et al) is used under MIT. Serilog is used under the Apache License Version 2.0, January 2004.

"JW Library" is a registered trademark of Watch Tower Bible and Tract Society of Pennsylvania.
