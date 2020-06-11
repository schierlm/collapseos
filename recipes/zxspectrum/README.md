# Sinclair ZX Spectrum

Sinclair ZX Spectrum is the British Z80-based home computer manufactured and sold in app. 5 mln branded units since the early 1980s and available worldwide in various clones until today. The most widespread legacy 48K model is a challenging Collapse OS target due to:
* absense of standard communication protocols, with the expansion egde connector requiring complex additional hardware;
* cassette tape storage as the only medium supported out-of-the-box;
* 16K ROM space with no paging, 7K memory-mapped screen, and the infamous [contended memory][contention], limiting the resources further against the competitors.

This recipe builds a RAM-based kernel self-assembling on the 48K model with tape storage. The ZX Spectrum firmware routines are used extensively for console and tape handling, which requires the `IM 2` interrupt mode of the Z80. Tape is accessed in two ways, via a read-only block device abstraction (required for `zasm`) and a set of short read/write applications directly bridging the MMAP blkdev and file system to tape. The assembly of the kernel binary is done with the tape-based CFS as the source and the bare MMAP blkdev as the destination. `Zasm`, `ed`, and tape applications are included into the RAM kernel binary. The MMAP size is 16K, the remaining userspace is app 7K.

## Gathering parts

* A Sinclair ZX Spectrum with access to tape storage. The 48K and 128K+ models have EAR/MIC [3.5 mm sockets][sockets] on board, the +3 has a single input/output socket, the +2A needs restoration of the EAR input, the +2/+2B models have a built-in tape recorder (reportedly out of order on most currently available units) and no sockets, which requires restoration of the pinout.
* A tape recorder with blank tapes, or a digital wave playback/recording device
* A cord pluggable into the playback/recording device and the ZX tape sockets
* Tape conversion routines for modern hardware, such as [BIN2TAP][bin2tap] for tape format conversion of the kernel binary and [TAPIR][tapir] for direct playback or WAV conversion of the tape image (both under Windows). See the description of [tape file format][TAP]
* A hex file editor for SD card image manipulations

## Kernel assembly

The necessary kernel modules include grid display emulation (32x21) and keyboard scan routine, both using the firmware ROM calls for simplicity and thus requiring preservation of the `IY` register extensively used by Spectrum BASIC. RAM-based kernels have to work in `IM 2` mode for this, with the handler located in the non-contended memory above `0x8000`. The `IM 2` [interrupt vector table][im2readmore] should be placed in the non-contended memory as well for the [128K model compatibility][128Kim2].

Stack memory is contended in this recipe, with the bottom at `0x8000`, which should be changed (e.g. by patching the kernel init routine before launching) if precise timings are needed. The top of the user memory `0xc000` is one possible bottom of the moved stack.

The tape-based blkdev is another platform-specific kernel module. The device is read-only due to infeasibility of tape-to-tape assembly. Instead, MMAP-tape bridge routines are designed as applications independent of the tape blkdev, but included into the binary in this recipe. They naturally require the MMAP blkdev in the kernel.

To save some RAM for MMAP, the shell and ed line buffers are slightly reduced compared to defaults (2K `BUF_POOLSIZE` left for 256 lines with shell, 3K `ED_BUF_PADMAXLEN` for 1024 `ED_BUF_MAXLINES` lines with ed). The 1K shell `BUF_LINES` buffer is placed to contended memory. 

Due to features of screen handling by firmware ROM, the three bottom screen rows are left blank.

See the respective glue.asm and user.h files for details.

## Assembly of application binaries

The application binaries, `zxs/ed.bin`, `zxs/zasm.bin` and `zxs/tapeutil.bin` were assembled in advance and included into glue.asm using `.bin` directive.

The starting point is learning `BAS_RAMEND` value by examining a memory dump of the test build assembled with the necessary shell memory allocation. On init, the basic shell saves the SP value, which is the value set in glue.asm minus 2 (`0x7ffe` in this recipe). The address of this storage +6 is `BAS_RAMEND`, which is the reference point for all memory assignments for included binaries. The tape blkdev and utilities use 527 bytes in this build, which allows assigning memory from `BAS_RAMEND+527` to `USER_CODE`, incl. `ed` and `zasm` working buffers, by writing the value in `user.h`.

Finally, the binaries lengths should be used to learn the necessary .org values for each application. The `.org` and `.equ USER_RAMSTART USER_CODE` directives in `mono/ed` and `mono/zasm` glue files override the `user.h` settings.

## Tape conversion

The kernel binary can be converted to a tape file with a single CODE-type block and a header using the utilities described above or similar.
Conversion of the OS sources to the tape-based CFS is, however, a tedious two-step process and requires:
* splitting the SD card image, using a hex editor, into smaller parts of size equal to MMAP volume of your build, containing an integer number of CFS blocks;
* converting each part to a tape image with a single headerless block;
* loading the blocks into the ZX Spectrum Collapse OS MMAP filesystem using the `BINLD` application (see below);
(with a Spectrum emulator such as Fuse, the two previous steps can be shortcut by loading image parts into MMAP area directly using the `File/Load binary data` option)
* saving them to tape from the MMAP using the `CFSSV` application (see below), as a chain of 256-byte headerless blocks.
The resulting sequence of blocks is readable by the tape blkdev.
To save some assembly time, it is advised to minimize the working tapes content: one tape per application, one tape for kernel sources of the currently processed build.

The tape files with the OS sources converted from the emulator cfs images (as standing on May, 2020) are:
* `Kernel.tap` (Spectrum kernel modules, utilities, basic shell, old shell)
* `Apps.tap` (ed, zasm, memt, sdct, at28w)
* `Hardware.tap` (non-Spectrum kernel modules)
in this directory. 

## Launching

To load and run the kernel from tape, execute the following in 48K BASIC mode of the Spectrum:

    CLEAR 24316
    LOAD "" CODE 24317
    RANDOMIZE USR 24317

You may adjust your screen colours using `INK/PAPER/BRIGHT/BORDER` and `BORDCR (23624)` variable prior to that. 
An example of a loader package is shown as `Boot.tap` in this directory.  

## Emulation

The recipe has been tested to work on the emulated ZX Spectrum under Windows, using Fuse 1.1.1 binary build and the original Sinclair Research Ltd ROMs. The original 48K and 128K configurations both run the boot binary, starting from either 48K or 128K BASIC mode. The binary works with the Beta Disk/TR-DOS emulation turned either on or off. No other emulated clones were tested.

To run the provided package or your converted boot tape, open the `Boot.tap` using the `Media/Tape/Open (F7)` command in Fuse and run the launching commands above. Depending on Fuse configuration, the binary file will be loaded instantly or in real time.

Please take note of the following when using emulated tapes with Collapse OS under Fuse:
* The `Use tape traps` option in `Options/General (F4)` menu has to be turned on when *saving* to an emulated tape. Before loading from the tape just created, always save the modified content to a new .tap file by `Media/Tape/Write (F6)` and re-open it by `Media/Tape/Open (F7)`
* When learning the system with minor works, the `Use tape traps` option can be turned off when *loading* from the tape. The playback is started and stopped manually by `Media/Tape/Play (F8)` in this case. Do not rewind the tape unless prompted by the magenta border
* When assembling from the tape with `zasm`, the `Use tape traps` option can be turned on for speed, as the tape is always rewound after loading a new data portion
* The new content is written at the end of the .tap file and can not be erased or overwritten
* The `CFSSV` and `CUTSV` utilities add a 2-second pause between the saved CFS blocks, which is required for `zasm` to load and process the blocks seamlessly, but not preserved in .tap files. It is not relevant to tape emulation, but if you plan to convert your .tap files for real hardware, the pauses have to be somehow restored.

## Usage

The devices list is as follows:
* 0 - MMAP,
* 1 - file handle 0,
* 2 - file handle 1,
* 3 - tape blkdev.

CFS is initialized on MMAP upon launch. Major assembly works with zasm should be done from tape using the bare MMAP blkdev (0) as the destination.

The tape CFS is not default, it has to be mounted by typing `bsel 3: fson`, then `fls`. File listing should be completed by loading a null ("invalid") block. It's useful to have at least one at the end of every tape, including the boot tape. Other tape operations which get rid of the placeholder data in the buffer, including `ed` or `fopen`, will suffice.

File handles are then assigned by typing fopen and loading the first block of the file. When searching for a tape block, purple border means "rewind the tape and press ENTER". Writing does not work with the tape device (specifically, PutB points to unsetZ with all due consequences). For creation of new tape files, MMAP and the dedicated routines have to be used.

In case the CFS 'magic' label is damaged in the tape buffer due to a loading error, fs commands will error out. To restore the buffer, type `bseek 256: getb: bseek 0: getb` to reload the first CFS sector from tape, then `fls` or `ed` the first file before starting any assembly works.

The MMAP-tape routines, exposed as shell commands in this recipe, are as follows (optional arguments are in parentheses; only decimal values are accepted):
* `BINSV (len)` - saving the MMAP content as a raw headerless binary, the whole MMAP area by default
*(may be convenient for storing the existing MMAP filesystem within a single block, saving the freshly assembled kernel binary, creating a null block)*
* `BINLD len` (a/o, a by default) - raw binary loading, appending or overwriting the filesystem
*(may be useful for loading the filesystem converted from an SD card image, or a previously stored MMAP)*
* `HEAD addr len` - saving of the CODE-block header in the Spectrum BASIC format
*(required for loading and launching the self-assembled kernel binary from Spectrum BASIC)*
* `FILSV file` - saving a particular file as a raw headerless binary
*(useful for exporting a file from the tape CFS)*
* `FILLD filename len (a/o, a by default)` - loading of the raw binary as a file into the existing MMAP filesystem, appending or overwriting the filesystem
*(for importing an external source file or binary)*
* `CFSSV (file)` - saving a particular file or (by default) the whole existing filesystem as a chain of 256-byte blocks *(to be readable by the tape blkdev)*
* `CUTSV filename (len)` - saving the raw MMAP content (whole MMAP area by default) as a single CFS file, cutting it into a chain of blocks *(for saving an application binary assembled to bare MMAP. Saving an existing filesystem will error out)*
* `CFSLD (num, 1 by default (a/o, a by default))` - loading of the CFS chain, appending or overwriting the filesystem
*(useful for viewing, editing and converting source files, particularly kernel sources)*

For getting the length of a freshly assembled binary, ZTELL routine is exposed, fetching the internal writing counter from within the zasm working memory. It can be accessed by executing `addr ztell: s=a`, then `usr s: print h`.

## Modifications and further development

Some additional memory can be acquired by reducing the grid display to 32x15 (2K + 256b additionally) or 32x7 (4K + 512b additionally) size and thus freeing a part of the screen memory. The bottom row in the last third of the screen used for grid emulation has to be left blank. The two memory blocks are located *down* from `0x5800` (bitmap) and `0x5b00` (attributes) respectively in the contended memory and should be managed accordingly (e.g. for placing interrupt and jump tables upon boot; for `basic`/`ed` line buffers etc.).

All of the advanced models and clones are backwards-compatible with the 48K model, making this recipe a suitable point of departure. 

There is a number of external disk systems for the Spectrum. Many of them (e.g. the +3's native +3DOS, or Beta Disk interface implemented in many Eastern European clones), while removing the need for included applications, may still require `IM 2` mode, and thus the 'non-contended memory', even with completely stand-alone console drivers, due to the interface architecture.

The 128K/+2/+3 models have a combined MIDI/RS232 port on board, with the pin diagrams widely available. This allows writing serial I/O drivers pluggable into the `IM 2` interrupt handler, expanding usability in various ways.

The extended RAM of the 128K+/+2/+3 models may be accessed by modifying the MMAP kernel module to directly support RAM paging. This may be useful for viewing and editing large source files, but is not enough for memory-to-memory kernel reassembly, as the minimal set of kernel sources is app. 160K. The ROM/RAM paging available on the +3 model allows the kernel to reside in RAM under `0x0`. Some of the paged RAM is contended on all mass produced models.

More sophisticated tape bufferization, with the additional RAM footprint, may increase the speed of tape-to-memory self-assembly, which is almost prohibitively slow at the moment.

The 16K model would require a ROM-based kernel completely replacing the firmware for Collapse OS to remain self-hosted. It can however be expanded with the additional RAM module up to 32K plugged to the edge connector.

[bin2tap]: https://sourceforge.net/p/zxspectrumutils/wiki/bin2tap/
[tapir]: http://www.worldofspectrum.org/pub/sinclair/tools/pc/tapir1.0.zip
[TAP]: https://faqwiki.zxnet.co.uk/wiki/TAP_format
[im2readmore]: http://www.breakintoprogram.co.uk/computers/zx-spectrum/interrupts
[sockets]: https://faqwiki.zxnet.co.uk/wiki/Tape_leads
[contention]: https://www.worldofspectrum.org/faq/reference/48kreference.htm#Contention
[128Kim2]: https://www.worldofspectrum.org/faq/reference/128kreference.htm
