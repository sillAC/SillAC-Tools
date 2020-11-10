# Sill's AC Tools  (WIP)



## Profiles

* Reloads upon change
* Trigger can be customized, but by default it is `/xp`
* Searched in order of descending priority
* Defaults are attempted to be created if the path is  missing
* **Character profiles** match a more specific profile by an optional regex of their name, account, and the server name.



You can access the main profile and the character's profile with:

`/xp editconfig`

`/xp editpolicy`



## AutoXP

[Demo video](https://streamable.com/yt0gwf)

Spend available experience based on a ratio you define.  The higher the number the more experience you'd spend on the skill (e.g., War - 10, Endurance - 1 would level War if it cost less than 10x Endurance).

View loaded policy in game:

`/xp policy`

View plan to spend experience:

`/xp plan`

Spend / stop spending experience:

`/xp level`

`/xp spend`



## Spellbar Manager

[Demo video](https://streamable.com/xu3ca5)

*This only works up to the 7th spell bar as far as I know.*



Save all spells (default path "Spells.json"):

`/xp ss [path]`

Clear all spells:

`/xp cs`

Load spells (takes time to add to bar, default path "Spells.json"):

`/xp ls [path]`



## Login Commands

The character profiles can include a file that will be loaded 5 seconds (for now) after logging in using AC's `/loadfile [batch file]`

There's other stuff out there that accomplishes the same, but this lets you do it based on the name/account/server regex, which gives a different (easier for me) way of grouping characters together.

![Login demo](/Media/LoginCommandDemo.png)

