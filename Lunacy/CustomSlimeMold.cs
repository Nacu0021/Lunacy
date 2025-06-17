using DevInterface;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Lunacy
{
    public class CustomSlimeMoldHooks
    {
        public static ConditionalWeakTable<PlacedObject.Data, CustomSlimeMoldSettings> CustomSlimeSettings = new();
        public static ConditionalWeakTable<AbstractConsumable, CustomSlimeMold> CustomSlime = new();
        public static ConditionalWeakTable<SlimeMold.CosmeticSlimeMold, StrongBox<bool>> CustomCosmeticSlime = new();
        public static void Apply()
        {
            // Initializing and saving the data
            On.PlacedObject.GenerateEmptyData += PlacedObject_GenerateEmptyData;
            On.SaveState.SetCustomData_Data_string += SaveState_SetCustomData_Data_string;
            On.PlacedObject.ConsumableObjectData.FromString += ConsumableObjectData_FromString;
            On.SaveState.SetCustomData_AbstractPhysicalObject_string += SaveState_SetCustomData_AbstractPhysicalObject_string;
            IL.SaveState.AbstractPhysicalObjectFromString += AbstractPhysicalObjectFromStringIL;
            IL.Room.Loaded += Room_Loaded;
    
            // Setting the data in devtools
            On.DevInterface.ConsumableRepresentation.ConsumableControlPanel.ctor += ConsumableControlPanel_ctor;
    
            // Functionality
            On.SlimeMold.PlaceInRoom += SlimeMold_PlaceInRoom;
            On.SlimeMold.ApplyPalette += SlimeMold_ApplyPalette;
            IL.SlimeMold.CosmeticSlimeMold.ApplyPalette += CosmeticSlimeMold_ApplyPalette;

            // SlimeMoldLight fixture
            On.SlimeMoldLight.ApplyPalette += SlimeMoldLight_ApplyPalette;
        }

        private static void SlimeMoldLight_ApplyPalette(On.SlimeMoldLight.orig_ApplyPalette orig, SlimeMoldLight self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (self is EffectSlimeMoldLight e)
            {
                e.ApplyPalettEffect(sLeaser, rCam, palette);
                return;
            }

            orig.Invoke(self, sLeaser, rCam, palette);
        }

        public class EffectSlimeMoldLight : SlimeMoldLight
        {
            public bool A;

            public EffectSlimeMoldLight(Room placedInRoom, PlacedObject placedObject, PlacedObject.LightFixtureData lightData, bool A) : base(placedInRoom, placedObject, lightData)
            {
                this.A = A;
            }

            public void ApplyPalettEffect(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
            {
                darkness = palette.darkness;
                for (int i = 0; i < sLeaser.sprites.Length; i++)
                {
                    sLeaser.sprites[i].color = rCam.currentPalette.texture.GetPixel(30, A ? 4 : 2);
                }
            }
        }

        private static void Room_Loaded(ILContext il)
        {
            ILCursor c = new(il);
            if (c.TryGotoNext(
                x => x.MatchLdsfld<PlacedObject.Type>("SlimeMold")
                ) && 
                c.TryGotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<Room>("AddObject")
                ))
            {   
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc, 27); // This will break in the update. This did break in the update...
                c.EmitDelegate<Action<Room, int>>((room, i) =>
                {
                    if (room.updateList.Last() is SlimeMold.CosmeticSlimeMold slime && !CustomCosmeticSlime.TryGetValue(slime, out _))
                    {
                        PlacedObject placedObject = room.roomSettings.placedObjects[i];
                        if (CustomSlimeSettings.TryGetValue(placedObject.data, out var cubed) && cubed.colorType != CustomSlimeMoldSettings.ColorType.Default)
                        {
                            CustomCosmeticSlime.Add(slime, new StrongBox<bool>(cubed.colorType == CustomSlimeMoldSettings.ColorType.EffectA));
                        }
                    }
                });
            }
            else Plugin.logger.LogMessage("CosmeticSlimeMold Room_Loaded 1 fucked up " + il);

            ILCursor b = new(il);
            if (b.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld<PlacedObject.Type>("LightFixture")
                ))
            {
                b.Index += 2;
                b.Emit(OpCodes.Ldarg_0);
                b.Emit(OpCodes.Ldloc, 27);
                b.EmitDelegate<Action<Room, int>>((room, num10) =>
                {
                    if ((room.roomSettings.placedObjects[num10].data as PlacedObject.LightFixtureData).type == LunacyEnums.SlimeMoldLightA)
                    {
                        room.AddObject(new EffectSlimeMoldLight(room, room.roomSettings.placedObjects[num10], room.roomSettings.placedObjects[num10].data as PlacedObject.LightFixtureData, true));
                    }
                    else if ((room.roomSettings.placedObjects[num10].data as PlacedObject.LightFixtureData).type == LunacyEnums.SlimeMoldLightB)
                    {
                        room.AddObject(new EffectSlimeMoldLight(room, room.roomSettings.placedObjects[num10], room.roomSettings.placedObjects[num10].data as PlacedObject.LightFixtureData, false));
                    }
                });
            }
            else Plugin.logger.LogMessage("CosmeticSlimeMold Room_Loaded 2 fucked up " + il);

        }

        private static void CosmeticSlimeMold_ApplyPalette(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<SlimeMold>("SlimeMoldColorFromPalette")
                ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldarg_2);
                c.EmitDelegate<Func<Color, SlimeMold.CosmeticSlimeMold, RoomCamera, Color>>((orig, self, rCam) =>
                {
                    if (CustomCosmeticSlime.TryGetValue(self, out var cubed))
                    {
                        orig = rCam.currentPalette.texture.GetPixel(30, cubed.Value ? 4 : 2);
                    }
                    return orig;
                });
            }
            else Plugin.logger.LogMessage("CosmeticSlimeMold_ApplyPalette fucked up " + il);
        }

        public static void SlimeMold_PlaceInRoom(On.SlimeMold.orig_PlaceInRoom orig, SlimeMold self, Room placeRoom)
        {
            orig.Invoke(self, placeRoom);
    
            if (!self.AbstrConsumable.isConsumed && self.AbstrConsumable.placedObjectIndex != -1 && self.AbstrConsumable.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count && placeRoom.roomSettings.placedObjects[self.AbstrConsumable.placedObjectIndex].data is PlacedObject.ConsumableObjectData c && CustomSlimeSettings.TryGetValue(c, out var slime))
            {
                if (slime.big)
                {
                    self.big = true;
                }
                if (!CustomSlime.TryGetValue(self.AbstrConsumable, out _))
                {
                    CustomSlime.Add(self.AbstrConsumable, new CustomSlimeMold() { big = self.big });
                }
            }
            else if (CustomSlime.TryGetValue(self.AbstrConsumable, out var cubed))
            {
                self.big = cubed.big;
            }
        }
    
        public static void ConsumableControlPanel_ctor(On.DevInterface.ConsumableRepresentation.ConsumableControlPanel.orig_ctor orig, ConsumableRepresentation.ConsumableControlPanel self, DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string name)
        {
            orig.Invoke(self, owner, IDstring, parentNode, pos, name);
    
            if (CustomSlimeSettings.TryGetValue((parentNode as ConsumableRepresentation).pObj.data, out var slime))
            {
                self.subNodes.Add(new CustomButtonBecauseConsumableControlPanelIsntIDevUISignals(owner, "CustomSlime_ToggleBig", self, new Vector2(5f, 45f), 110f, slime.big ? "Big" : "Normal size", slime));
                self.subNodes.Add(new CustomButtonBecauseConsumableControlPanelIsntIDevUISignals(owner, "CustomSlime_ToggleColor", self, new Vector2(135f, 45f), 110f, slime.colorType.ToString(), slime));
                self.size.y = 65f;
            }
        }
    
        public class CustomButtonBecauseConsumableControlPanelIsntIDevUISignals : Button
        {
            public CustomSlimeMoldSettings slime;
    
            public CustomButtonBecauseConsumableControlPanelIsntIDevUISignals(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, float width, string text, CustomSlimeMoldSettings slime) : base(owner, IDstring, parentNode, pos, width, text)
            {
                this.slime = slime;
            }
    
            public override void Clicked()
            {
                base.Clicked();
                if (IDstring == "CustomSlime_ToggleBig")
                {
                    slime.big = !slime.big;
                    Text = slime.big ? "Big" : "Normal size";
                }
                else if (IDstring == "CustomSlime_ToggleColor")
                {
                    int g = (int)slime.colorType + 1;
                    slime.colorType = (CustomSlimeMoldSettings.ColorType)(g > 2 ? 0 : g);
                    Text = slime.colorType.ToString();
                }
            }
        }
    
        public static void SlimeMold_ApplyPalette(On.SlimeMold.orig_ApplyPalette orig, SlimeMold self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig.Invoke(self, sLeaser, rCam, palette);
    
            if (self.AbstrConsumable != null && self.room != null)
            {
                if (CustomSlime.TryGetValue(self.AbstrConsumable, out var slim) && slim.color != default)
                {
                    self.color = slim.color;
                }
                else if (slim != null && self.AbstrConsumable.placedObjectIndex != -1 && self.AbstrConsumable.originRoom == self.room.abstractRoom.index && self.room.roomSettings.placedObjects[self.AbstrConsumable.placedObjectIndex].data is PlacedObject.ConsumableObjectData c && CustomSlimeSettings.TryGetValue(c, out var slime) && slime.colorType != CustomSlimeMoldSettings.ColorType.Default)
                {
                    slim.color = rCam.currentPalette.texture.GetPixel(30, slime.colorType == CustomSlimeMoldSettings.ColorType.EffectA ? 4 : 2);
                    self.color = slim.color;
                }
            }
        }
    
        public static void ConsumableObjectData_FromString(On.PlacedObject.ConsumableObjectData.orig_FromString orig, PlacedObject.ConsumableObjectData self, string s)
        {
            orig.Invoke(self, s);
            if (CustomSlimeSettings.TryGetValue(self, out var slime) && self.unrecognizedAttributes != null)
            {
                foreach (string data in self.unrecognizedAttributes) // mmyes compatibility
                {
                    if (data.Contains("<lc>"))
                    {
                        string[] a = Regex.Split(data, "<lc>");
                        slime.colorType = (CustomSlimeMoldSettings.ColorType)int.Parse(a[0]);
                        slime.big = a[1] == "1";
                    }
                }
            }
        }

        public static void AbstractPhysicalObjectFromStringIL(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(
                x => x.MatchNewobj<AbstractConsumable>()
                ) && c.TryGotoNext(MoveType.After,
                x => x.MatchStfld<AbstractPhysicalObject>("unrecognizedAttributes")
                ))
            {
                c.Emit(OpCodes.Ldloc, 5);
                c.EmitDelegate<Action<AbstractPhysicalObject>>((self) =>
                {
                    if (self.unrecognizedAttributes != null)
                    {
                        foreach (string data in self.unrecognizedAttributes) // mmyes compatibility
                        {
                            if (data.Contains("<lc>"))
                            {
                                string[] a = Regex.Split(data, "<lc>");
                                if (!CustomSlime.TryGetValue(self as AbstractConsumable, out var slime))
                                {
                                    CustomSlime.Add(self as AbstractConsumable, new CustomSlimeMold() { big = a[1] == "1", color = ColorFromString(a[0]) });
                                }
                                else
                                {
                                    slime.color = ColorFromString(a[0]);
                                    slime.big = a[1] == "1";
                                }
                            }
                        }
                    }
                });
            }
            else Plugin.logger.LogMessage("AbstractPhysicalObjectFromStringIL failed! " + il);
        }

        public static string SaveState_SetCustomData_Data_string(On.SaveState.orig_SetCustomData_Data_string orig, PlacedObject.Data pod, string baseString)
        {
            string text = orig.Invoke(pod, baseString);
            if (CustomSlimeSettings.TryGetValue(pod, out var slime) && !text.Contains("<lc>"))
            {
                text += "~" + (int)slime.colorType + "<lc>" + (slime.big ? "1" : "0");
                if (pod.unrecognizedAttributes != null)
                {
                    List<string> grug = pod.unrecognizedAttributes.ToList();
                    grug.RemoveAll(x => x.Contains("<lc>"));
                    pod.unrecognizedAttributes = grug.ToArray();
                }
            }
            return text;
        }

        public static string SaveState_SetCustomData_AbstractPhysicalObject_string(On.SaveState.orig_SetCustomData_AbstractPhysicalObject_string orig, AbstractPhysicalObject apo, string baseString)
        {
            string text = orig.Invoke(apo, baseString);
            if (apo is AbstractConsumable c && CustomSlime.TryGetValue(c, out var slime) && !text.Contains("<lc>"))
            {
                text += string.Concat(
                    "<oA>",
                    slime.color.r,
                    "|",
                    slime.color.g,
                    "|",
                    slime.color.b,
                    "|",
                    slime.color.a,
                    "<lc>",
                    slime.big ? "1" : "0"
                    );
            }
            return text;
        }

        public static void PlacedObject_GenerateEmptyData(On.PlacedObject.orig_GenerateEmptyData orig, PlacedObject self)
        {
            orig.Invoke(self);
    
            if (self.type == PlacedObject.Type.SlimeMold && !CustomSlimeSettings.TryGetValue(self.data, out _))
            {
                CustomSlimeSettings.Add(self.data, new CustomSlimeMoldSettings());
            }
        }
    
        // from Sisus on stackoverflow, modified
        public static Color ColorFromString(string col)
        {
            string[] rgba = col.Split('|');
            return new Color(float.Parse(rgba[0]), float.Parse(rgba[1]), float.Parse(rgba[2]), float.Parse(rgba[3]));
        }
    }
    
    public class CustomSlimeMoldSettings
    {
        public ColorType colorType;
        public enum ColorType
        {
            Default,
            EffectA,
            EffectB,
        }
        public bool big;
    
        public CustomSlimeMoldSettings()
        {
            colorType = ColorType.Default;
        }
    }
    
    public class CustomSlimeMold
    {
        public Color color;
        public bool big;
    
        public CustomSlimeMold()
        {
        }
    }
}
