Kessler Chaos
=============

A mod that adds more deadly space debris to Kerbal Space Program.

How it works
------------

Adds random debris cloud encounters when flying. The likelyhood of
encountering a debris cloud depends on:

* how much "normal" debris you have floating around the sphere of
influence (up to a max of 250)
* your altitude (more the lower you are, but none below
atmosphere/terrain)
* how much time has elapsed since the last debris cloud encounter

Debris clouds are also more intense depending on the amount of
"normal" debris in the SOI. This should give you an incentive to keep space
clean (or not depending on your proclivities).

Upcoming feature wishlist
-------------------------

* Real trajectories for debris clouds rather than random encounters
* Modeled chain reactions for a proper Kessler Syndrome
* Improved graphical effects
* Sound effects for the small impacts that don't create explosions
* Allow debris clouds to affect "on rails" vessles
* Debris detector vessel parts to provide alarms and early warnings
* Debris mitigation vessel parts such as laser brooms and "deflector dishes"

Build and Installation
----------------------

To install from the pre-built zip file, extract the zip to the
"GameData" subdirectory of your Kerbal Space Program install.

To build from source, open the solution with monodevelop.
Make sure the references to Assembly-CSharp.dll and UnityEngine.dll come
from ksp's KSP_Data/Managed/ directory. Build it then copy
kesslerchaos.dll from the output directory to ksp's GameData folder.
Use the Makefile to generate a release zipfile.

Licenses
--------

Kessler Chaos copyright 2014 Daniel Kinsman  
GNU General Public License v3

KSP Plugin Framework copyright 2014 TriggerAu  
MIT License  
https://ksppluginframework.codeplex.com/

ToolbarWrapper copyright 2013-2014 Maik Schreiber  
BSD 2-Clause License  
https://github.com/blizzy78/ksp_toolbar

Known Issues
------------

See https://github.com/DanielKinsman/kessler-chaos/issues

Changelog
---------

v0.6:

* Landed, splashed down and sub-orbital debris no longer counts

v0.4:

* Debris cloud forecast shown to user
* GUI button to force debris cloud encounter
* Adjustable settings per savegame
* Better collisions (less misses)
* Toolbar integration
* Time warp blocked during debris cloud encounters
* Allowed mod to work with mods that alter planet dimensions
* Uses game simulation time instead of wall clock time

v0.2:

* First release

Contact
-------

* danielkinsman@riseup.net
* https://github.com/DanielKinsman/kessler-chaos


