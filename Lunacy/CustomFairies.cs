using Menu.Remix;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lunacy
{
    public class CustomFairies
    {
        public static Dictionary<PlacedObject.FairyParticleData, List<string>> customSprite = [];
        public static void Apply()
        {
            // Drawing custom sprite
            IL.PlacedObject.FairyParticleData.Apply += FairyParticleData_Apply;

            // Assigning custom sprite from text
            IL.PlacedObject.FairyParticleData.FromString += FairyParticleData_FromString;
            IL.MoreSlugcats.FairyParticleRepresentation.FairyParticleControlPanel.Signal += FairyParticleControlPanel_Signal;

            // Saving custom sprite
            IL.PlacedObject.FairyParticleData.BaseSaveString += FairyParticleData_BaseSaveString;
        }

        public static void FairyParticleData_BaseSaveString(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdfld<PlacedObject.FairyParticleData>("spriteType")
                ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<PlacedObject.FairyParticleData.SpriteType, PlacedObject.FairyParticleData, string>>((orig, self) =>
                {
                    if (orig == LunacyEnums.Custom && customSprite.ContainsKey(self))
                    {
                        string save = "Custom";
                        foreach (string s in customSprite[self])
                        {
                            save += "|" + s;
                        }
                        return save;
                    }
                    return ValueConverter.ConvertToString(orig);
                });
            }
            else Plugin.logger.LogMessage("FairyParticleData_BaseSaveString FAILED " + il);
        }

        public static void FairyParticleData_Apply(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchStfld<MoreSlugcats.FairyParticle>("rotation_rate")
                ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc_2);
                c.Emit(OpCodes.Ldloc_3);
                c.EmitDelegate<Action<PlacedObject.FairyParticleData, FairyParticle, PlacedObject.FairyParticleData.SpriteType>>((self, fairy, type) =>
                {
                    if (type == LunacyEnums.Custom && customSprite.ContainsKey(self))
                    {
                        int sprites = customSprite[self].Count;
                        float r = UnityEngine.Random.value;
                        for (int i = 0; i < sprites; i++)
                        {
                            if (r < ((float)(i + 1) / (float)sprites))
                            {
                                fairy.spriteName = customSprite[self][i];
                                break;
                            }
                        }
                        fairy.scale_multiplier = 0.25f;
                    }
                });
            }
            else Plugin.logger.LogMessage("FairyParticleData_Apply FAILED " + il);
        }

        public static void FairyParticleControlPanel_Signal(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(
                x => x.MatchLdstr("Sprite_Button")
                ) && c.TryGotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<ExtEnumType>("GetEntry")
                ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<string, PlacedObject.FairyParticleData, string>>((orig, self) =>
                {
                    AddThing(self, ref orig);
                    return orig;
                });
            }
            else Plugin.logger.LogMessage("FairyParticleControlPanel_Signal FAILED " + il);
        }

        public static void FairyParticleData_FromString(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchNewobj<PlacedObject.FairyParticleData.SpriteType>()
                ))
            {
                c.Index--;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<string, PlacedObject.FairyParticleData, string>>((orig, self) =>
                {
                    AddThing(self, ref orig);
                    return orig;
                });
            }
            else Plugin.logger.LogMessage("FairyParticleData_FromString FAILED " + il);
        }

        public static void AddThing(PlacedObject.FairyParticleData d, ref string s)
        {
            if (s.StartsWith("Custom"))
            {
                customSprite[d] = new();
                string[] custom = s.Split('|');
                if (custom.Length > 1)
                {
                    for (int i = 1; i < custom.Length; i++)
                    {
                        customSprite[d].Add(custom[i]);
                    }
                }
                else customSprite[d].Add("buttonCrossB");
                s = "Custom";
            }
        }
    }
}
