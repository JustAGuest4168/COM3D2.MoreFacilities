# COM3D2.MoreFacilities

UPDATES
=======
# 2020-05-16:

V1.0.1.0 Release

Bug fix to vanilla save file, the check for 12 facilities in previous version caused the error and was unneccessary. All tests have been reperformed and verified this time.

# 2020-05-15:

Official first release

Updated README and LICENSE

Minor bug fix to when there are less than 12 facilities

# 2020-04-18:

Modification to how data is saved, creates an additional file with the additional facilities and casino dealer data.

Originally the plan was to just keep the relevant data in extra files, but the whole file needs to be saved because dealer data is not deserializing properly during load (but it does deserialize properly during save).

Maximum Facilty Count increased from 50 to 60

# 2020-04-15:

Fix for facility upgrades maid task

# 2020-04-14: 

First Alpha


WARNING
==========================================================
The following COM3D2 Plugin.

My plugin development is limited, by proceeding you agree that you will not hold me, any of the plugin tool developers, or KISS responsible for anything that happens to your PC or game data.

It is always recommended that you make a backup of game files before testing, save frequently, and save to a new save slot in case of any bugs.

REQUIREMENTS
==========================================================
COM3D2 Version 1.45.0 or later

BepinEx Version 5.0.1 https://github.com/BepInEx/BepInEx

INSTALL
==========================================================
Place COM3D2.MoreFacilities.Plugin.dll in your BepinEx\plugins directory before starting your game (eg. C:\KISS\COM3D2\BepinEx\plugins)

USAGE DESCRIPTION
==========================================================
This plugin increases the maximum facility count to 60. 

This works in Facility Manager, Life Mode (GP01), and Guest Mode.

On any page with the Facility Grid (my name for it, not official), use the Up/Down Arrow Keys or the Mouse's Scroll Wheel to browse all facilities. 
All of the facilities you add should be available to Maid Scheduling, as well as triggering end-of-day event unlocks.

[EDIT][2020-04-15]:
Fix to allow scrolling during the Facility Updgrades Maid Task, note that only the mouse wheel works here.

LEGAL
==========================================================
GNU Lesser General Public License v3.0

NOTES
==========================================================
Please contact Guest4168 on the Discord to report any comments, suggestions, and errors (including in this readme).

FUTURE UPDATES
==========================================================
Facility Re-Skinning
-Hoping to implement a way to re-skin facilities so that the "luxury" version of facilities can use different textures for existing models and maybe add user-defined additional props (fancy hotel bed, stage chandelier, etc.). Maybe even use the Facility Recipe for determining how to re-skin.

Active Plugin Indicator
-Add some UI indicator that this plugin is active. Right now there is not a scrollbar or anything that tells you have more facilities and that you can find them by scrolling or using arrow keys. I'm also thinking that it would be nice to have a list of events for Life Mode on the side, right now you have to scroll through every facility to review all the available events. Suggestiongs for implementing this feature are welcome. 

Page Scrolling
-May allow scrolling to be adjustable in future so that you scroll through pages of facilities instead of rows.

DISTANT FUTURE UPDATES
==========================================================
Facility Creator
-Tools for creating custom facilities for daily work, Life Mode, yotogi, etc..

