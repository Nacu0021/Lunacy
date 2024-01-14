# Lunacy
Library of custom features for Rain World, whether that'd be creatures, cosmetic insects, items, or whatever feature i feel like adding. 

# Features

## New cosmetic bugs :))
Available inside the `InsectGroup` object in devtools, and as room effects.

### Lightning bugs!
![bugspart2electricbugaloo](https://user-images.githubusercontent.com/67332756/217871778-cb31a469-18c5-4a2c-9a56-7157bcafa57d.gif)

### Funny starfish!!
![fis](https://github.com/Nacu0021/Lunacy/assets/67332756/d6c9e865-5afc-4958-947e-b5deb286bb9d)

## Firebatflies!!
![firebatflies](https://github.com/Nacu0021/Lunacy/assets/67332756/2a7312f5-07b8-44ec-a718-8368e22a45de)

You can make any room tagged as `: SWARMROOM` have glowing batflies instead of standard ones!
To do this, add `: FIREFLIES` after any instance of `: SWARMROOM` you desire.
You can also specify the hue of these batflies with `: FIREFLIES|hue` where hue is a number from **0 to 360**, or you may instead specify the effect color of the room with `: FIREFLIES|A` where the last letter is either **A or B**.
For example:
```
SU_A40 : SU_A17, SU_B07 : SWARMROOM : FIREFLIES|150
SU_A06 : SU_A39, SU_A36, SU_A38 : SWARMROOM : FIREFLIES|B
```
If you don't specify a color, the batflies will instead be the following color

![firebatfliestwo2](https://github.com/Nacu0021/Lunacy/assets/67332756/6b0e3ae9-5406-4094-9d1f-34e936fa2a64)

## Mod Conditional Spawns
Here are some normal spawns:
```
GW_C08 : 3-Leech-10, 5-Snail-4
(White,Red)SL_D06 : 7-Salamander
(White)LINEAGE : GW_B02 : 4 : White-0.3, BigSpider-0.3, SpitterSpider-0
```

With this mod, you can filter spawns based on what other mods are enabled. For example, to only spawn leeches if More Slugcats Expansion is enabled:
```
GW_C08 : 3-Leech-10[moreslugcats], 5-Snail-4
```

The mod id filter goes after the number of creatures and/or after any spawn data. You can also write a short "MSC" or "DP" instead of "moreslugcats".

You may use the `|` symbol to set fallback spawns. If the specified mod isn't enabled, it uses the one to the right instead.  
With this feature you can put modded creatures in your region without depending on the other mod, and losing out on a creature!

Place the pipe symbol after the mod filter, and remember not to repeat the den number:
```
(White,Red)SL_D06 : 7-Aquacenti[MSC]|Salamander-2
```

Filters can be chained with multiple `|` symbols:
```
SL_D06 : 7-Aquacenti-2[MSC]|FunnyCreature-{PreCycle}[extracreatures]|Salamander
```

When modifying lineages, place the filter after the progression chance:
```
(White)LINEAGE : GW_B02 : 4 : Caramel-0.5[mycoolmod]|White-{Mean:0.5}-0.3, BigSpider-0.3, SpitterSpider-0
```

To spawn a creature only if a certain mod is disabled, use `NONE`:
```
GW_C08 : 3-NONE[MSC]|Leech-10, 5-Snail-4
```
