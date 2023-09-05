using UnityEngine;
using RWCustom;
using System.Collections.Generic;

namespace Lunacy.Insects
{
    public class Starfish : CosmeticInsect // Seastar is a better name boo-whomp. actually both are good actually
    {
        public Vector2 dir;
        public Vector2 rot;
        public Vector2 lastRot;
        public Color color;
        public Vector2[] fins;
        public float[] flap;
        public List<int> nextFlaps;
        public float size;
        public float rotSpeed;
        public int flapFalloff;
        public int testcounter; // Turns out its not a test counter but im not changing the name
        public bool behavior;

        public LightSource light;

        public Starfish(Room room, Vector2 pos) : base(room, pos, LunacyEnums.Starfish)
        {
            creatureAvoider = new CreatureAvoider(this, 8, 150f, 0.4f);
            color = LightningBug.RandomizeColorABit(new Color(0.5f, 1f, 0.5f), 0.033f, 0.15f);
            size = Mathf.Lerp(3.6f, 5f, Random.value);
            flap = new float[5] { 1f, 1f, 1f, 1f, 1f };
            fins = new Vector2[5];
            for (int i = 0; i < 5; i++)
            {
                fins[i] = Custom.DegToVec(i * 72);
            }
            nextFlaps = new List<int>();
            testcounter = Random.Range(-10, 40);
        }

        public override void EmergeFromGround(Vector2 emergePos)
        {
            base.EmergeFromGround(emergePos);
            dir = Custom.RNV();
            rot = dir;
            Flap();
        }

        public override void Update(bool eu)
        {
            lastRot = rot;

            base.Update(eu);

            if (light == null)
            {
                light = new LightSource(pos, false, color, this);
                room.AddObject(light);
            }
            light.setPos = new Vector2?(pos);
            light.setAlpha = new float?(0.3f + 0.1f * size);//new float?(room.Darkness(pos));
            light.setRad = new float?(size * 27f);
            light.affectedByPaletteDarkness = 0.5f;

            for (int i = 0; i < 5; i++)
            {
                fins[i] = Custom.DegToVec(Custom.VecToDeg(rot) + i * 72);
            }

            if (!room.BeingViewed)
            {
                Destroy();
            }
        }

        public override void Act()
        {
            base.Act();
            vel *= 0.95f;

            rot = Custom.DegToVec(Custom.VecToDeg(rot) + rotSpeed);
            rotSpeed = Mathf.Lerp(rotSpeed, 0f, 0.005f);

            if (submerged)
            {
                dir += Custom.RNV() * Random.value * 0.5f; 
                // A bit stolen from WaterGlowworms hihihi
                if (this.wantToBurrow)
                {
                    dir.y -= 0.5f;
                }
                if (this.pos.x < 0f)
                {
                    dir.x += 1f;
                    Flap();
                }
                else if (this.pos.x > this.room.PixelWidth)
                {
                    dir.x -= 1f;
                    Flap();
                }
                if (this.pos.y < 0f)
                {
                    dir.y += 1f;
                    Flap();
                }
                if (this.creatureAvoider.currentWorstCrit != null)
                {
                    dir -= Custom.DirVec(this.pos, this.creatureAvoider.currentWorstCrit.DangerPos) * this.creatureAvoider.FleeSpeed;
                    testcounter++;
                }
                else
                {
                    if (mySwarm != null && !room.IsPositionInsideBoundries(room.GetTilePosition(pos)))
                    {
                        dir = Vector2.Lerp(this.dir, Custom.DirVec(this.pos, this.mySwarm.placedObject.pos), Mathf.InverseLerp(this.mySwarm.insectGroupData.Rad, this.mySwarm.insectGroupData.Rad + 100f, Vector2.Distance(this.pos, this.mySwarm.placedObject.pos)));
                    }
                    if (base.OutOfBounds)
                    {
                        this.dir = Vector2.Lerp(this.dir, Custom.DirVec(this.pos, this.mySwarm.placedObject.pos), Mathf.InverseLerp(this.mySwarm.insectGroupData.Rad, this.mySwarm.insectGroupData.Rad + 100f, Vector2.Distance(this.pos, this.mySwarm.placedObject.pos)));
                    }
                }
                if (this.room.water)
                {
                    dir = Vector3.Slerp(dir, new Vector2(0f, -1f), Mathf.InverseLerp(this.room.FloatWaterLevel(this.pos.x) - 100f, this.room.FloatWaterLevel(this.pos.x), this.pos.y) * 0.05f);
                }
                if (room.GetTile(pos + (dir * 40f)).Solid || !room.GetTile(pos + (dir * 40f)).AnyWater)
                {
                    dir = Vector3.Slerp(dir, -dir, 0.3f + (0.4f * Random.value));
                }
                this.dir.Normalize();
            }
            else
            {
                vel += new Vector2(0, -0.3f);
            }

            testcounter++;
            if (testcounter >= 60)
            {
                if (flapFalloff == 0)
                {
                    Flap();
                    testcounter = Random.Range(-10, 40);
                }
            }

            if (flapFalloff > 0)
            {
                for (int i = 0; i < flap.Length; i++)
                {
                    if (nextFlaps.Contains(i))
                    {
                        bool reverse = flapFalloff < 14;
                        flap[i] = Mathf.InverseLerp(13, reverse ? 0 : 25, flapFalloff);
                    }
                }

                flapFalloff--;
            }

            if (flapFalloff == 0 && nextFlaps.Count > 0)
            {
                for (int i = 0; i < flap.Length; i++) flap[i] = 1f;
                nextFlaps.Clear();
            }
        }

        public void Flap()
        {
            vel += dir * (size + Random.value + (Random.value * Random.value)) / 2f;
            rotSpeed += (Random.value < 0.5f ? -1f : 1f) * (3f + Random.value + (Random.value * Random.value));
            flapFalloff = 25;

            // Figure out which fins to flap
            for (int i = 0; i < fins.Length; i++)
            {
                if (Vector2.Distance(fins[i], -dir) < 1.3f) nextFlaps.Add(i);
            }

            // Extra visuals heho
            if (Random.value < .33f)
            {
                room.AddObject(new Spark(pos, -vel / 2f + Custom.RNV(), new Color(0.01f, 0.01f, 0.01f), null, 10, 17));
                room.PlaySound(SoundID.Slugcat_Bite_Water_Nut, pos);
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[6];

            sLeaser.sprites[0] = new FSprite("Circle20", true)
            {
                scale = (1f / 20f) * size
            };

            for (int i = 1; i < 6; i++)
            {
                sLeaser.sprites[i] = new FSprite("ShortcutArrow", true)
                {
                    scaleX = (1f / 9f) * size * 1.2f,
                    scaleY = (1f / 5f) * size * 1.8f
                };
            }

            base.InitiateSprites(sLeaser, rCam);
            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            Vector2 positio = Vector2.Lerp(lastPos, pos, timeStacker);
            float rotatio = Custom.VecToDeg(Vector2.Lerp(lastRot, rot, timeStacker));

            sLeaser.sprites[0].SetPosition(positio - camPos);
            sLeaser.sprites[0].rotation = rotatio;

            for (int i = 0; i < 5; i++)
            {
                Vector2 dirr = Custom.DegToVec(rotatio + (i * 72)); // 72 degrees

                sLeaser.sprites[i + 1].SetPosition(positio + dirr * (size / 2f) - camPos);
                sLeaser.sprites[i + 1].rotation = Custom.VecToDeg(dirr);
                sLeaser.sprites[i + 1].scaleY = (1f / 5f) * size * Mathf.Lerp(0.5f, 1.8f, flap[i]);
            }
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].color = color;
            }
            if (light != null)
            {
                light.color = color;
            }
        }
    }
}
