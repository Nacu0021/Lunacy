using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lunacy
{
    public class Metropolis
    {
        public static Dictionary<string, Color[]> lightningGradientColors = new();

        public static void Apply()
        {
            IL.Room.Loaded += Room_Loaded;
        }

        private static void Room_Loaded(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchStfld<Room>("lightning")))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<Room>>((self) =>
                {
                    if (lightningGradientColors.TryGetValue(self.world.name, out Color[] colors))
                    {
                        //foreach (Color c in colors) { Plugin.logger.LogMessage("COLOURE: " + c); }
                        if (colors[0] != default) self.lightning.bkgGradient[0] = colors[0];
                        if (colors[1] != default) self.lightning.bkgGradient[1] = colors[1];
                        //Plugin.logger.LogMessage($"L color 1: {self.lightning.bkgGradient[0]}. L color 2: {self.lightning.bkgGradient[1]}");
                    }
                });
            }
            else Plugin.logger.LogError("Lunacy Room_Loaded lightning hook failed!");
        }
    }
}
