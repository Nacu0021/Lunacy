using static Pom.Pom;
using PomCore;
using UnityEngine;
using RWCustom;

namespace Lunacy.PlacedObjects
{
    using PhysicalObjects;
    using static PlacesObjects;

    public class CoralSpearCoral : CosmeticSprite
    {
        public PlacedObject obj;
        public CoralSpearAbstract coralSpear;
        public int placedObjIndex;
        public Vector2 rot;
        public bool placed = false;

        public CoralSpearCoral(PlacedObject obj, Room room)
        {
            this.obj = obj;
            this.room = room;
            pos = obj.pos;
            rot = (obj.data as ManagedData).GetValue<Vector2>("rot").normalized;

            placedObjIndex = -1;
            for (int i = 0; i < room.roomSettings.placedObjects.Count; i++)
            {
                if (room.roomSettings.placedObjects[i] == obj)
                {
                    placedObjIndex = i;
                    break;
                }
            }
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            pos = obj.pos;
            rot = (obj.data as ManagedData).GetValue<Vector2>("rot").normalized;
            //room.AddObject(new Spark(pos, rot * 10f * Random.value, Color.cyan, null, 10, 20));

            if (coralSpear == null)
            {
                if (!placed && placedObjIndex != -1 && ((room.game.session is not StoryGameSession) || (room.game.session is StoryGameSession s && !s.saveState.ItemConsumed(room.world, false, room.abstractRoom.index, placedObjIndex))))
                {
                    coralSpear = new CoralSpearAbstract(room.world, null, room.GetWorldCoordinate(pos), room.game.GetNewID())
                    {
                        stuck = true
                    };
                    coralSpear.RealizeInRoom();
                    (coralSpear.realizedObject as CoralSpear).coral = this;
                    placed = true;
                }
            }
            else
            {
                CoralSpear s = (coralSpear.realizedObject as CoralSpear);
                if (s.snapCounter == 0)
                {
                    s.setRotation = new Vector2?(rot);
                    s.rotationSpeed = 0;
                }
                s.firstChunk.MoveFromOutsideMyUpdate(eu, pos + rot * 20f);
                s.firstChunk.vel *= 0f;
                //(coralSpear.realizedObject as CoralSpear).mode = Weapon.Mode.Free;
            }
        }

        public void SnapOff()
        {
            CoralSpear s = (coralSpear.realizedObject as CoralSpear);

            s.coral = null;
            coralSpear.stuck = false;
            if (room.game.session is StoryGameSession g) g.saveState.ReportConsumedItem(room.world, false, room.abstractRoom.index, placedObjIndex,
                Random.Range((obj.data as POMConsumableData).GetValue<int>("mincycles"), (obj.data as POMConsumableData).GetValue<int>("maxcycles") + 1));

            Creature grabbber = s.grabbedBy[0].grabber;

            grabbber.mainBodyChunk.vel += new Vector2(0f, 5f) + Custom.DirVec(Vector2.Lerp(pos, s.firstChunk.pos, 0.75f), grabbber.mainBodyChunk.pos) * 12f;

            for (int i = 0; i < Random.Range(4, 9); i++)
            {
                room.AddObject(new Spark(pos, rot * 4f + Custom.RNV(), Color.cyan, null, 14, 22));
            }
            room.PlaySound(SoundID.Fire_Spear_Pop, pos, 0.66f, 1.33f);

            coralSpear = null;
        }
    }
}
