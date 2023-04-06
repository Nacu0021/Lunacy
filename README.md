# Lunacy
Library of custom features for Rain World, whether that'd be creatures, cosmetic insects, items, or whatever feature i feel like adding. 

# Features

## Lightning bugs :))
Available inside the `InsectGroup` object in devtools, and as room effects.

![bugspart2electricbugaloo](https://user-images.githubusercontent.com/67332756/217871778-cb31a469-18c5-4a2c-9a56-7157bcafa57d.gif)


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

The filter goes after the number of creatures and/or after any spawn data. You can also write a short "MSC" or "DP" instead of "moreslugcats".

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
