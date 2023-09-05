using static Pom.Pom;
using UnityEngine;
using DevInterface;
using RWCustom;

namespace Lunacy.PlacedObjects
{
    public class PlacesObjects//Gently
    {
        public static void Apply()
        {
            RegisterManagedObject(new CoralSpearPlacedType());
        }

        // POM business

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

        public class CoralSpearPlacedType : ManagedObjectType
        {
            public CoralSpearPlacedType() : base("CoralSpearCoral", "Lunacy", null, typeof(CoralSpearPlacedData), typeof(CoralSpearCoralRep))
            {
            }

            public override UpdatableAndDeletable MakeObject(PlacedObject placedObject, Room room)
            {
                return new CoralSpearCoral(placedObject, room);
            }
        }

        public class CoralSpearPlacedData : POMConsumableData
        {
            [Vector2Field("rot", 0f, 40f)]
            public Vector2 rot;

            public CoralSpearPlacedData(PlacedObject owner) : base(owner)
            {
            }
        }

        public class CoralSpearCoralRep : ManagedRepresentation
        {
            public CoralSpearCoralRep(PlacedObject.Type type, ObjectsPage page, PlacedObject obj) : base(type, page, obj)
            {
            }

            public override void Update()
            {
                base.Update();
                (pObj.data as CoralSpearPlacedData).rot = Custom.DirVec(absPos, absPos + (pObj.data as CoralSpearPlacedData).rot) * 40f;
            }
        }
    }
}
