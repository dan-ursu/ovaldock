# OvalDock

The newest version can be downloaded from here:

https://github.com/dan-ursu/OvalDock/releases/latest

The TLDR is open tutorial.jpg


## About

OvalDock is a program inspired by Circle Dock, which appears to have been abandoned since around 2008 and can still be found here:

http://circledock.wikidot.com/

Both programs give you the ability to create a circular dock around your mouse cursor, and add shortcuts to anything you want on it. Think of it like the existing start menu, but with more a more convenient shape and functionality.

Although it is visually inspired by Circle Dock, OvalDock is actually written entirely from scratch without taking any inspiration from the source code of Circle Dock. Even the default icons are designed from scratch.

OvalDock aims to make some necessary improvements to the original Circle Dock. While it does not (yet!) have the entire feature set of the original program, some of the major improvements it DOES make, which from what I can tell are lacking in Circle Dock, are the following:

* It is FAST. Circle Dock, for some reason, will easily take 20+ seconds on initial launch. OvalDock will do this in a couple of seconds. Furthermore, OvalDock will cache ALL icons it will use on startup, so switching between dock folders is instantaneous. There is a noticeable delay when doing this with Circle Dock.

* It (mostly) uses WPF as opposed to the older Windows Forms. In particular, it is much better at handling high resolution scaling. Circle Dock would end up looking "pixellated".


## Installation

Just copy the OvalDock folder anywhere that is convenient. To launch OvalDock, just run OvalDock.exe.

Currently, the program cannot configure itself to run on startup. You will need to configure that manually yourself using your favourite method. Windows 10 overcomplicates this, so I won't explain it here.


## Usage

See tutorial.jpg for the TLDR. For more detailed instructions:

* Ctrl+Win will show/hide the dock. The dock will always center itself around your mouse cursor when being shown.

* Drag items onto the dock to place them on it. They will automatically get placed in clockwise order, equally spaced, starting from the rightmost edge.

* Drag items already on the dock around to reposition them.

* Left click on an item to launch it.

* Right click an item for more options, including removing it.

* The dock supports nested dock folders (these are NOT folders on your storage drive - these are for organizing dock items!). To add one, right click on the dock, click "Add Item", and make sure you select "Dock folder".

* To enter a dock folder, click on it. To go back one folder, click on the icon in the middle. If you are in the root folder, this will hide the dock.
