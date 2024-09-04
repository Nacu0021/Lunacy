using static Pom.Pom;
using UnityEngine;

namespace Lunacy.PlacedObjects
{
    public class PlacesObjects//Gently
    {
        public static void Apply()
        {
            RegisterManagedObject(new CustomSlimeMoldPlacedType());
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
        }
    }
}
