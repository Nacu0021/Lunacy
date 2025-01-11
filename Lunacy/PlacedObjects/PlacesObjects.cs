using static Pom.Pom;
using UnityEngine;
using Lunacy.CustomTokens;
using System.Collections.Generic;

namespace Lunacy.PlacedObjects
{
    public class PlacesObjects//Gently
    {
        public static void Apply()
        {
            RegisterManagedObject(new CustomSlimeMoldPlacedType());

            //ManagedField[] ventFields =
            //{
            //    new FloatField("angle", 0f, 360f, 0f, increment: 15f, ManagedFieldWithPanel.ControlType.arrows, displayName: "Push/Pull angle"),
            //    new FloatField("strength", -10f, 10f, 0f, 0.25f, ManagedFieldWithPanel.ControlType.slider, "Push/Pull strength"),
            //    new BooleanField("push", true, displayName: "Push physical objects"),
            //    new FloatField("air", 0f, 1f, 1f, 0.05f, control: ManagedFieldWithPanel.ControlType.slider, displayName: "Air refill strength"),
            //    new BooleanField("bubbles", true, displayName: "Spawn bubbles"),
            //    new BooleanField("sound", true, displayName: "Play sound"),
            //    new FloatField("pitch", 0f, 1f, 1f, 0.05f, control:ManagedFieldWithPanel.ControlType.slider, displayName: "Pitch"),
            //    new Vector2Field("handle", new Vector2(60f, 60f), Vector2Field.VectorReprType.rect)
            //};
            //ManagedField[] circleVentFields =
            //{
            //    new FloatField("air", 0f, 1f, 1f, 0.05f, control: ManagedFieldWithPanel.ControlType.slider, displayName: "Air refill strength"),
            //    new BooleanField("bubbles", true, displayName: "Spawn bubbles"),
            //    new BooleanField("sound", true, displayName: "Play sound"),
            //    new FloatField("pitch", 0f, 1f, 1f, 0.05f, control:ManagedFieldWithPanel.ControlType.slider, displayName: "Pitch"),
            //    new Vector2Field("handle", new Vector2(60f, 60f), Vector2Field.VectorReprType.circle)
            //};
            //
            //RegisterFullyManagedObjectType(ventFields, typeof(AirVentCurrent), "RectVentCurrent", "Lunacy");
            //RegisterFullyManagedObjectType(circleVentFields, typeof(CircleVent), "CircVent", "Lunacy");

            On.Room.Loaded += NonPomPlacedObjects;
        }

        public static void NonPomPlacedObjects(On.Room.orig_Loaded orig, Room room)
        {
            //bool firstTime = room.abstractRoom.firstTimeRealized;
            orig.Invoke(room);
            if (room.game == null) return;
            if (room.roomSettings != null && room.roomSettings.placedObjects != null && room.roomSettings.placedObjects.Count > 0)
            {
                for (int i = 0; i < room.roomSettings.placedObjects.Count; i++)
                {
                    PlacedObject obj = room.roomSettings.placedObjects[i];
                    foreach (string key in LunacyTokens.CustomTokenDefinitions.Keys)
                    {
                        if (obj.type.value == key)
                        {
                            Plugin.logger.LogWarning("yippee " + key);

                            if (!(room.game.session is StoryGameSession) || 
                                room.world.singleRoomWorld || 
                                !LunacyTokens.GetCustomTokenCollected((room.game.session as StoryGameSession).game.rainWorld.progression.miscProgressionData, key, (obj.data as CustomCollectTokenData).tokenString))
                            {
                                room.AddObject(new CustomCollectToken(room, obj));
                            }
                            else
                            {
                                room.AddObject(new CustomTokenStalk(room, obj.pos, obj.pos + (obj.data as CustomCollectTokenData).handlePos, null));
                            }
                        }
                    }
                }
            }
        }

        public class CustomCosmeticSlimeMoldData : ManagedData
        {
            [BooleanField("effectA", true, displayName: "Effect Color A")]
            public bool EffectA;
            [IntegerField("moldtype", 1, 2, 1, ManagedFieldWithPanel.ControlType.arrows, displayName: "Type")]
            public int One;
            [Vector2Field("radius", 50f, 0f, Vector2Field.VectorReprType.circle)]
            public Vector2 Rad;
    
            public CustomCosmeticSlimeMoldData(PlacedObject owner) : base(owner, new ManagedField[] { })
            {
            }
        }

        public class CustomSlimeMoldPlacedType : ManagedObjectType
        {
            public CustomSlimeMoldPlacedType() : base("CustomCosmeticSlimeMold", "Lunacy", null, typeof(CustomCosmeticSlimeMoldData), typeof(ManagedRepresentation))
            {
            }

            public override UpdatableAndDeletable MakeObject(PlacedObject placedObject, Room room)
            {
                Plugin.logger.LogMessage($"Mold positions: {placedObject.pos}, {(placedObject.data as ManagedData).GetValue<Vector2>("radius")}");
                SlimeMold.CosmeticSlimeMold slime = new SlimeMold.CosmeticSlimeMold(room, placedObject.pos, Vector2.Distance(placedObject.pos, placedObject.pos + (placedObject.data as ManagedData).GetValue<Vector2>("radius")), (placedObject.data as ManagedData).GetValue<int>("moldtype") == 2);
                if (!CustomSlimeMoldHooks.CustomCosmeticSlime.TryGetValue(slime, out _)) CustomSlimeMoldHooks.CustomCosmeticSlime.Add(slime, new System.Runtime.CompilerServices.StrongBox<bool>((placedObject.data as ManagedData).GetValue<bool>("effectA")));
                return slime;
            }
        }

        public class POMConsumableData : ManagedData
        {
            [IntegerField("mincycles", 0, 50, 1, ManagedFieldWithPanel.ControlType.slider, "Min Cycles:")]
            public int minCycles;
        
            [IntegerField("maxcycles", 0, 50, 3, ManagedFieldWithPanel.ControlType.slider, "Max Cycles:")]
            public int maxCycles;
        
            public POMConsumableData(PlacedObject owner) : base(owner, new ManagedField[] { })
            {
            }

            public override void RefreshLiveVisuals()
            {
                base.RefreshLiveVisuals();
            }
        }
    }
}
