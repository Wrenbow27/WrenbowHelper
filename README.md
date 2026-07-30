# WrenbowHelper
Just some stuff that I need for mapping
Warning: Probably jank :3

-Technical Details-

Dream block lift speed fix:
Fixes the oversight of dream blocks not giving liftspeed when the player moves both into and out of them within a single frame
This occurs most notably with moving reverse dream blocks

Dream jump refill:
Allows for a second non-buffered dream jump i.e. dtjs
This works by refunding the coyote time of a jump/hyper/super if it was done using the dream block's coyote time
While a dream jump is "stored" like this, the flag WrenbowHelper_HasDreamJump is held as true

Bluetooth refill:
Requested by jantheguy
A refill that can also be collected by holdables

Set LiftSpeed trigger:
Overides normal liftspeed inheritence with a custom value (very silly)

Rubiks Cubes:
Initialise and view the puzzle with the entity, and interact with it using the watchtower, turn trigger, or scramble trigger
Sets a flag when the puzzle is cleared; use this wisely
Or not, I'm not the boss of you
A colorblind mode is provided under Mod Settings
Yes, I know that nobody asked for this. ANd yes, I do think it's funny

-Credits-

Chronia Helper - moreOptions lua code

Extended Variants Mode - adapted Jump Refill textures

Communal Helper - adapted Dream Refill textures
