# Lunacy
Library of custom features for Rain World, whether that'd be creatures, cosmetic insects, items, or whatever feature i feel like adding. 

# Features

## Lightning bugs :))
Available inside the `InsectGroup` object in devtools, and as room effects.

![bugspart2electricbugaloo](https://user-images.githubusercontent.com/67332756/217871778-cb31a469-18c5-4a2c-9a56-7157bcafa57d.gif)


## Mod Conditional Spawns
With this mod it's possible to write checks in the `CREATURES` section of the world file whether a mod is active or not, and depending on the outcome, a creature can either not appear at all or be switched to a different one. Let me show a few examples.  
**(both modify files and normal world_xx files work for this)**


### Making a creature spawn be dependant on specific mod

We have this line in the `CREATURES` section of `world_gw`.
```
GW_C08 : 3-Leech-10, 5-Snail-4
```
Let's say we're evil and want to punish people using the Downpour DLC.  
More specifically we want the leech creature spawn (`3-Leech-10`) to only appear if the More Slugcats Expansion is active.

To do this, first we have to get the **ID** of More Slugcats, mod **ID**s are stored in the `modinfo.json` files inside any mod's folder.  
Before the comma, we add in `[moreslugcats]`. The text [*between the square brackets*] specifies the **ID** of the mod we are looking to check for, if a mod with that **ID** is active, the creature will spawn, if not - the creature spawn will be ignored. 
```
GW_C08 : 3-Leech-10[moreslugcats], 5-Snail-4
```
Now, the 10 leeches will be there only if you're using the More Slugcats Expansion! Truly pay to lose!

For the More Slugcats Expansion specifically, there exists a shorthand version of it's **ID**, so you don't have to type it all out.  
It's either `MSC` or `DP`, so both examples below will function the same as the example above.
```
GW_C08 : 3-Leech-10[MSC], 5-Snail-4  =  GW_C08 : 3-Leech-10[DP], 5-Snail-4
```
<sub>Note: this specific example will not work in reality because one of moreslugcats's modify files removes this entire line from existance, but you get the idea.</sub>

### Changing a creature spawn if specific mod is not active

This time, let's assume we want to use Downpour's Aquatic Centipede creature instead of a Salamander Lizard, when the DLC is active.  
Let's take this line inside `world_sl` as an example.
```
(White,Red)SL_D06 : 7-Salamander
```
To do this one, first, after the den number (`7-` in our case) we write the creature's name we want to use instead of the `Salamander`, in our case its `Aquacenti`.  
After that, we specify our mod **ID** in [square brackets] like in the previous example, in our case either `[moreslugcats]`, `[MSC]`, or `[DP]` works.  
And after specifying the **ID**, we put a pipe (`|`) symbol, which essentially means "if the previously specified mod isn't active, use the text after this symbol instead".

So, now our line reads:
```
(White,Red)SL_D06 : 7-Aquacenti[MSC]|Salamander
```
Again, this means the game will spawn an Aquatic Centipede in this den if More Slugcat's Expansion is active, and a Salamander Lizard if it's not.

It's important to put the square brackets and pipe symbol after specifying the creature count, any `{}` and after the lineage chance.  
Also remember to **not** repeat the den number, the den number stays at the start only else the entire game will explode probably.

So these two:
```
GW_E02 : 8-Mimic-{14}
(White)LINEAGE : GW_B02 : 4 : White-0.3, BigSpider-0.3, SpitterSpider-0
```
*Should* end up looking like this for example: 
```
GW_E02 : 8-Mimic-{14}[MSC]|Mimic-{140}
(White)LINEAGE : GW_B02 : 4 : Caramel-0.5[com.mycoolmod]|White-0.3, BigSpider-0.3, SpitterSpider-0
```
**It is important to note that you need to specify a new lineage chance also along the name of the creature in a lineage chain.**

You can also nest these mod checks
```
GW_A04 : 3-Red Centipede[com.skrunklymod]|Aquacenti-2[com.scrimblomod]|Small Centipede-3
```
So in this case, if a mod with an **ID** of `com.skrunklymod` is active - a red centipede will spawn, if not then if the `com.scrimblomod` is active two aqua centipedes will spawn, if not to both of those then three small centipedes will spawn. And so on.

If you need to ask a question or more examples, @ me in the rain world discord (@Nacu0021)
