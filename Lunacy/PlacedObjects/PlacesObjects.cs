using static Pom.Pom;
using UnityEngine;
using DevInterface;
using RWCustom;

namespace Lunacy.PlacedObjects
{
    using PhysicalObjects;

    //public class PlacesObjects//Gently
    //{
    //    public static void Apply()
    //    {
    //        RegisterManagedObject(new CoralSpearPlacedType());
    //    }
    //
    //    // POM business
    //
    //    public class POMConsumableData : ManagedData
    //    {
    //        [IntegerField("mincycles", 0, 50, 1, ManagedFieldWithPanel.ControlType.slider, "Min Cycles:")]
    //        public int minCycles;
    //
    //        [IntegerField("maxcycles", 0, 50, 3, ManagedFieldWithPanel.ControlType.slider, "Max Cycles:")]
    //        public int maxCycles;
    //
    //        public POMConsumableData(PlacedObject owner) : base(owner, new ManagedField[] { })
    //        {
    //        }
    //    }
    //
    //    public class CoralSpearPlacedType : ManagedObjectType
    //    {
    //        public CoralSpearPlacedType() : base("CoralSpear", "Lunacy", null, typeof(CoralSpearPlacedData), typeof(CoralSpearCoralRep))
    //        {
    //        }
    //
    //        public override UpdatableAndDeletable MakeObject(PlacedObject placedObject, Room room)
    //        {
    //            if (room.abstractRoom.firstTimeRealized || room.world.singleRoomWorld)
    //            {
    //                CoralSpearAbstract spear = new CoralSpearAbstract(room.world, null, room.GetWorldCoordinate(placedObject.pos), room.game.GetNewID(), Random.Range(30, 46)) 
    //                { 
    //                    stuck = true,
    //                    placedObjectIndex = room.roomSettings.placedObjects.IndexOf(placedObject)
    //                };
    //                room.abstractRoom.AddEntity(spear);
    //                spear.RealizeInRoom();
    //            }
    //
    //            return null;
    //        }
    //    }
    //
    //    public class CoralSpearPlacedData : POMConsumableData
    //    {
    //        [Vector2Field("rot", 0f, 40f)]
    //        public Vector2 rot;
    //        [FloatField("hue", 0f, 1f, 0.5f, 0.01f)]
    //        public float hue;
    //        [FloatField("colorStart", 0f, 1f, 0.2f, 0.01f)]
    //        public float colorStart;
    //
    //        public CoralSpearPlacedData(PlacedObject owner) : base(owner)
    //        {
    //        }
    //    }
    //
    //    public class CoralSpearCoralRep : ManagedRepresentation
    //    {
    //        public CoralSpearCoralRep(PlacedObject.Type type, ObjectsPage page, PlacedObject obj) : base(type, page, obj)
    //        {
    //        }
    //
    //        public override void Update()
    //        {
    //            base.Update();
    //            // Limit the vector
    //            (pObj.data as CoralSpearPlacedData).rot = Custom.DirVec(absPos, absPos + (pObj.data as CoralSpearPlacedData).rot) * 40f;
    //        }
    //    }
    //}
}
