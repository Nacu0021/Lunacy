using UnityEngine;
using RWCustom;

namespace Lunacy.PhysicalObjects
{
    //using PlacedObjects;
    //using static Lunacy.PlacedObjects.PlacesObjects;
   // using static Pom.Pom;

    //public class CoralSpear : Spear
    //{
    //    //public CoralSpearCoral coral;
    //    public int snapCounter;
    //    public int maxSnapCounter;
    //    public bool increment;
    //    public PlacedObject obj;
    //    public Vector2 rot;
    //    public Color tipColor;
    //    public float hue;
    //
    //    public CoralSpearAbstract AbstractCoral { get; }
    //    public bool StuckToCoral => AbstractCoral.stuck;
    //
    //    public CoralSpear(CoralSpearAbstract abstr) : base(abstr, abstr.world)
    //    {
    //        AbstractCoral = abstr;
    //        maxSnapCounter = abstr.maxSnapCounter;
    //    }
    //
    //    public override void PlaceInRoom(Room placeRoom)
    //    {
    //        base.PlaceInRoom(placeRoom);
    //
    //        if (AbstractCoral.placedObjectIndex >= 0 && AbstractCoral.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
    //        {
    //            obj = placeRoom.roomSettings.placedObjects[AbstractCoral.placedObjectIndex];
    //            //coral = new CoralSpearCoral(placeRoom.roomSettings.placedObjects[AbstractCoral.placedObjectIndex], placeRoom, this);
    //            //placeRoom.AddObject(coral);
    //        }
    //    }
    //
    //    public override void Update(bool eu)
    //    {
    //        base.Update(eu);
    //
    //        if (increment)
    //        {
    //            snapCounter++;
    //            if (snapCounter > maxSnapCounter / 2) vibrate = 2;
    //            if (snapCounter > maxSnapCounter)
    //            {
    //                SnapOff();
    //            }
    //        }
    //        else snapCounter = 0;
    //        increment = false;
    //
    //        if (AbstractCoral.stuck)
    //        {
    //            rot = (obj.data as ManagedData).GetValue<Vector2>("rot").normalized;
    //
    //            if (snapCounter == 0)
    //            {
    //                setRotation = new Vector2?(rot);
    //                rotationSpeed = 0;
    //            }
    //            firstChunk.pos = obj.pos + rotation * 20f;
    //            firstChunk.vel *= 0f;
    //        }
    //    }
    //
    //    public override void SetRandomSpin()
    //    {
    //        if (AbstractCoral.stuck && snapCounter == 0) return;
    //        base.SetRandomSpin();
    //    }
    //
    //    public void SnapOff()
    //    {
    //        if (room.game.session is StoryGameSession g) g.saveState.ReportConsumedItem(room.world, false, room.abstractRoom.index, AbstractCoral.placedObjectIndex,
    //            Random.Range((obj.data as POMConsumableData).GetValue<int>("mincycles"), (obj.data as POMConsumableData).GetValue<int>("maxcycles") + 1));
    //
    //        Creature grabbber = grabbedBy[0].grabber;
    //
    //        grabbber.mainBodyChunk.vel += new Vector2(0f, 5f) + Custom.DirVec(Vector2.Lerp(obj.pos, firstChunk.pos, 0.75f), grabbber.mainBodyChunk.pos) * 12f;
    //
    //        for (int i = 0; i < Random.Range(4, 9); i++)
    //        {
    //            room.AddObject(new Spark(obj.pos, rot * 4f + Custom.RNV(), Color.cyan, null, 14, 22));
    //        }
    //        room.PlaySound(SoundID.Fire_Spear_Pop, obj.pos, 0.66f, 1.33f);
    //
    //        AbstractCoral.stuck = false;
    //    }
    //
    //    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    //    {
    //        sLeaser.sprites = new FSprite[1];
    //        sLeaser.sprites[0] = new FSprite("CoralSpear1");
    //        AddToContainer(sLeaser, rCam, null);
    //    }
    //
    //    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    //    {
    //        color = Color.cyan;
    //    }
    //}
}
