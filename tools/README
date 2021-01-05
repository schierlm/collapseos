# Tools

This folder contains tools to communicate to Collapse OS machines from a modern
environment or to manipulate a blkfs. 

Communication tools all take a device path as a first argument. That device is
the serial device that connects you to your machine. It's often a USB-to-TTL
dongle. When `-` is specified, `stdin` is used as the device.

Note that for these tools to work well, you need the serial device to be
properly set up, TTY-wise. You'll probably want to do that with `stty`. The tool
itself takes care of setting the regular stuff (`cs8`, `-parenb`, etc), but you
need to set the speed. Here's an example working on OpenBSD:

    $ ( stty 115200 raw ; sleep 2 ; ./upload - a000 os.bin ) <> /dev/cuaU0

To be honest, I'm having a bit of troubles making these tools work as well on
OpenBSD as they do in Linux. But it *does* work. Here are some advices:

* Use `cuaXX` instead of `ttyXX`.
* Run `cu -l /dev/cuaXX` before running your tool and run a dummy command to
  make sure that the output buffer is flushed.
* Use the "raw" option to avoid TTY-processing options to mess with data.
* If you experience random failures in your command, try inserting a "sleep 2"
  between your "stty" invocation and the command. In my experience, these tend
  to help.

On Linux, it's generally easier:

* Run screen on the device (often `/dev/ttyUSBX`)
* Quit with `CTRL+A :quit`
* Run the tool on the same device
