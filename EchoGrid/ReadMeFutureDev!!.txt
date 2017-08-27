To future developers and maintainers of this project:

This project is growing very large and fragile, so we need your help to organize it and keep development efforts smooth.
There have been many hacks, unused lines and unattributed copying in the source code and so far we could not fully address these problems.

At the moment, Doxygen documentation has been added to help describe the functionality of the files. Please update any mistakes you find and keep the documentation up-to-date, otherwise the project will become even harder to maintain.

To help future developers avoid getting lost, I have compiled this basic help page to guide you.
The topics are mostly based on questions I anticipate future developers will ask.

*** How is the code structured? ***
There are top level manager objects that start with "GM_" in their name. They handle menu interactions. The "Player" object also behaves like one of these "GM_" objects and is in general responsible for more aspects of the game than the name suggests. Class responsibility is not handled very well and usually a manager object modifies other objects in the game by searching for them by name and modifying their attributes from the outside. The game must be started from the "Title_Screen" scene or the "T&C" scene as there are persistent objects in the game that must be brought to the other scenes by scene transitions. The "T&C" scene doesn't work in the Unity editor, so it can only be tested on the phone.

*** The code takes 2 hours to compile. This is insane. ***
We think so too. This is caused by having too many audio files in the game. If you have the time, please figure out how to let the game use AssetBundles to avoid this compilation issue. In the meanwhile, there is a workaround. Move the Assets/Resources/echoes folder and its accompanying .meta file to /Resources-tempMoved. This disables the echoes in the game, but it means you can finally work on the program without having to spend hours compiling every time.

*** How do I use the editor to test the game? ***
Swiping up/down/left/right is mapped to the arrow keys. Press f to generate an echo in the game and press e to exit when you're on top of the exit sign. Press m to display the screen so that you don't have to bump around to solve the maze. Press r and left / up / right for other actions (up gives a hint, left returns to the main menu, and right goes to level 1).
If the game doesn't work in the editor, please try to fix it; debugging is impossible without it (we learned the hard way). It's probably caused by the preprocessing macros failing to detect that the game is running on editor mode. If you have time, try to reduce the code duplication for the menu code, which executes duplicated & nearly identical lines dependending on whether the game is running on the editor or on the phone.

*** Explain the "Logging" class ***
The game generates a large number of debug logs. To organize them, they are tagged with the importance of the log message. You can change the logging settings in the "Logging" class so that only messages beyond a certain priority level are displayed. We recommend that you avoid using Debug.Log directly in the future. Please delete logging calls that you really don't think need an entry in the log, because no log is still cleaner than a priority-tagged log.

*** How to use Doxygen? ***
There are online tutorials. Basically, you install Doxygen and a Unity editor extension. The Unity editor extension should install automatically because we put a file into Assets/Editors/Doxygen. In the worst case where you can't install Doxygen, you should still write the documentation comments with the right structure so that a future dev can run Doxygen on the project.

*** I can't enter the game and some weird error message is showing on a black screen ***
It's probably because the game disables itself until it detects an internet connection. Try connecting to Wi-Fi; 3G didn't work when I tried it.

*** How can I change the sprites back to the old version? ***
At the root directory you can fild the folders Sprites and Sprites_Old. Copy the contents into Assets/Sprites and the sprites will be changed. Do not touch the .meta files in the latter directory.

*** How can I disable the pink noise in the game? ***
Open the prefabs folder, select the "Exit" object and set the volume inside the Audio Source component to 0. Don't delete the Audio Source component or modify the curves unless you know what you are doing (it was kind of hard to set up that component).