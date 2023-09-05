using UnityEngine;
using RWCustom;
using System.Linq;
using MoreSlugcats;

namespace Lunacy.Insects
{
    public class LightningBug : CosmeticInsect
    {
        public Vector2 dir;
        public int zipCounter;
        public int maxzipCounter;
        public int canZip;
        public int currentZips;
        public LightSource light;
        public Color color;
        public Vector2 rot;
        public Vector2 lastRot;
        public int relevantCounter;
        public bool spawnAnother;
        public Vector2 lastPosForAnother;
        public bool A;
        public BodyChunk electricSpearHolder;

        public LightningBug(Room room, Vector2 pos, bool A) : base(room, pos, A ? LunacyEnums.LightningBugA : LunacyEnums.LightningBugB)
        {
            zipCounter = Random.Range(100, 700);
            maxzipCounter = zipCounter;
            relevantCounter = 10;
            this.A = A;
        }

        public override void EmergeFromGround(Vector2 emergePos)
        {
            base.EmergeFromGround(emergePos);
            dir = new Vector2(0f, 1f);
            rot = dir;
        }

        public override void Update(bool eu)
        {
            lastRot = rot;

            if (!this.alive)
            {
                if (light != null)
                { 
                    light.Destroy();
                    light = null;
                }
                vel.y -= 0.3f;
            }

            if (room.Darkness(pos) > 0f)
            {
                if (light == null)
                {
                    light = new LightSource(pos, false, color, this);
                    room.AddObject(light);
                }
                light.setPos = new Vector2?(pos);
                light.setAlpha = new float?(room.Darkness(pos));
                light.setRad = new float?(Custom.LerpMap(zipCounter, 70, 0, 70f, 10f, 0.75f));
            }
            else if (light != null)
            {
                light.Destroy();
                light = null;
            }

            base.Update(eu); 
            
            if (!room.BeingViewed)
            {
                Destroy();
            }
        }

        public override void Act()
        {
            base.Act();
            vel *= Custom.LerpMap(zipCounter, 70, 0, 0.85f, 0.55f, 0.75f);
            dir = Vector2.Lerp(this.dir, Custom.RNV() * Mathf.Pow(Random.value, 1.2f), Mathf.Pow(Random.value, 1.9f));
            if (wantToBurrow)
            {
                dir = Vector2.Lerp(dir, new Vector2(0f, -1f), 0.15f);
            }
            else if (this.electricSpearHolder != null)
            {
                if (!this.ViableForBuzzaround((this.electricSpearHolder.owner as Creature).abstractCreature))
                {
                    this.electricSpearHolder = null;
                }
                else if (!Custom.DistLess(this.pos, this.electricSpearHolder.pos + new Vector2(0f, this.electricSpearHolder.rad + 20f), this.electricSpearHolder.rad + 10f + Mathf.Pow(Random.value, 2f) * 100f))
                {
                    //this.vel += Custom.DirVec(this.pos, this.electricSpearHolder.pos + new Vector2(0f, this.electricSpearHolder.rad + 30f)) * 0.4f * Random.value;
                    this.dir = Vector2.Lerp(this.dir, Custom.DirVec(this.pos, this.electricSpearHolder.pos + new Vector2(0f, this.electricSpearHolder.rad)), 0.33f);
                }
            }

            if (mySwarm != null && !room.IsPositionInsideBoundries(room.GetTilePosition(pos)))
            {
                dir = Vector2.Lerp(this.dir, Custom.DirVec(this.pos, this.mySwarm.placedObject.pos), Mathf.InverseLerp(this.mySwarm.insectGroupData.Rad, this.mySwarm.insectGroupData.Rad + 100f, Vector2.Distance(this.pos, this.mySwarm.placedObject.pos)));
            }
            if (base.OutOfBounds)
            {
                this.dir = Vector2.Lerp(this.dir, Custom.DirVec(this.pos, this.mySwarm.placedObject.pos), Mathf.InverseLerp(this.mySwarm.insectGroupData.Rad, this.mySwarm.insectGroupData.Rad + 100f, Vector2.Distance(this.pos, this.mySwarm.placedObject.pos)));
            }
            else
            {
                //for (int i = 0; i < 4; i++)
                //{
                //    if (this.room.GetTile(this.room.GetTilePosition(this.pos) + Custom.fourDirections[i]).Solid ||
                //        this.room.GetTile(this.room.GetTilePosition(this.pos) + Custom.fourDirections[i] * 3).Solid)
                //    {
                //        var vektore = Custom.DirVec(pos, pos - Custom.fourDirections[i].ToVector2());
                //        dir = Vector2.Lerp(dir, vektore, 0.66f);
                //    }
                //}

                if (Random.value < 0.008333334f && this.room.abstractRoom.creatures.Count > 0)
                {
                    AbstractCreature abstractCreature = this.room.abstractRoom.creatures[Random.Range(0, this.room.abstractRoom.creatures.Count)];
                    if (abstractCreature.realizedCreature != null && Custom.DistLess(this.pos, abstractCreature.realizedCreature.firstChunk.pos, 250f + 250f * Random.value) && this.ViableForBuzzaround(abstractCreature) && this.room.VisualContact(this.pos, abstractCreature.realizedCreature.firstChunk.pos))
                    {
                        this.electricSpearHolder = abstractCreature.realizedCreature.bodyChunks[Random.Range(0, abstractCreature.realizedCreature.bodyChunks.Length)];
                    }
                }

                if (room.GetTile(pos + (dir * 20f)).Solid || room.GetTile(pos + (dir * 60f)).Solid) dir = Vector3.Slerp(dir, -dir, 0.33f);

                if (submerged) dir = Vector3.Slerp(dir, new Vector2(0, 1f), 0.1f);
            }

            rot = Vector3.Slerp(rot, dir, 0.1f);
            vel += rot * 1.5f;

            if (zipCounter > 0)
            {
                zipCounter--;

                if (spawnAnother && zipCounter == maxzipCounter - 3)
                {
                    room.AddObject(new StaticElectricty(lastPosForAnother + Custom.RNV() * 20 * Random.value, pos + Custom.RNV() * 20 * Random.value, Mathf.Lerp(0.25f, 0.66f, Random.value), 0.85f, color));
                    spawnAnother = Random.value < 0.66f;
                }
                if (spawnAnother && zipCounter == maxzipCounter - 5)
                {
                    room.AddObject(new StaticElectricty(lastPosForAnother + Custom.RNV() * 20 * Random.value, pos + Custom.RNV() * 20 * Random.value, Mathf.Lerp(0.2f, 0.7f, Random.value), 0.75f, color));
                    spawnAnother = false;
                }
                if (zipCounter < 50)
                {
                    if (relevantCounter > 0)
                    {
                        relevantCounter--;
                        if (relevantCounter == 0)
                        {
                            if (Random.value < 0.9f)
                            {
                                room.AddObject(new StaticElectricty(pos + Custom.RNV() * 10f, pos + Custom.RNV() * 10f, 0.6f, 0.3f, color));
                            }
                            else
                            {
                                room.AddObject(new MouseSpark(pos, Custom.RNV(), 20, RandomizeColorABit(color, 0.05f, 0.1f)));
                            }
                            relevantCounter = Random.Range(3, 10);
                        }
                    }
                }
            }
            if (zipCounter <= 0)
            {
                Zip();
            }
        }

        public void Zip()
        {
            float zipRange = Random.Range(220f, 400f);
            if (SharedPhysics.RayTraceTilesForTerrain(room, pos, pos + rot * zipRange))
            {
                room.AddObject(new StaticElectricty(pos, pos + rot * zipRange, Mathf.Lerp(1.5f, 2f, Random.value), 1f, color));
                room.PlaySound(SoundID.Spore_Bee_Spark, pos, 0.3f, 1.25f);
                lastPosForAnother = pos;
                pos += rot * zipRange;
                zipCounter = Random.Range(100, 700);
                maxzipCounter = zipCounter;
                spawnAnother = Random.value < 0.66f;
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);

            sLeaser.sprites = new FSprite[2];

            sLeaser.sprites[0] = new FSprite("pixel")
            {
                scaleX = 2f,
                scaleY = 6f
            };
            sLeaser.sprites[1] = new FSprite("pixel")
            {
                scale = 2f,
                anchorX = 0.5f,
                anchorY = 0.5f,
            };

            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            Vector2 positio = Vector2.Lerp(lastPos, pos, timeStacker);
            Vector2 rotatio = Vector2.Lerp(lastRot, rot, timeStacker);

            sLeaser.sprites[0].SetPosition(positio - camPos);
            sLeaser.sprites[0].rotation = Custom.VecToDeg(rotatio);
            sLeaser.sprites[1].SetPosition(positio - rot.normalized * 3f - camPos);
            sLeaser.sprites[1].rotation = Custom.VecToDeg(rotatio);
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            base.ApplyPalette(sLeaser, rCam, palette);
            color = RandomizeColorABit(RoomCamera.allEffectColorsTexture.GetPixel((A ? room.roomSettings.EffectColorA : room.roomSettings.EffectColorB) * 2, 0), 0.0f, 0.1f);
            sLeaser.sprites[0].color = palette.blackColor;
            sLeaser.sprites[1].color = color;
            if (light != null)
            {
                light.color = color;
            }
        }

        public static Color RandomizeColorABit(Color color, float factor1, float factor2)
        {
            var hslcolor = Custom.RGB2HSL(color);
            color = Custom.HSL2RGB(Custom.WrappedRandomVariation(hslcolor.x, factor1, factor2), hslcolor.y, Custom.ClampedRandomVariation(hslcolor.z, factor1, factor2));
            return color;
        }

        public bool ViableForBuzzaround(AbstractCreature crit)
        {
            return crit.realizedCreature != null && (crit.realizedCreature.grasps != null && ((ModManager.MSC && crit.realizedCreature.grasps.Any(x => x != null && x.grabbed != null && x.grabbed is ElectricSpear s && s.abstractSpear != null && s.abstractSpear.electricCharge > 0)) || crit.realizedCreature.grasps.Any(x => x != null && x.grabbed != null && x.grabbed is Centipede c && c.Small && c.dead)) && (this.mySwarm == null || Custom.DistLess(this.mySwarm.placedObject.pos, this.mySwarm.placedObject.pos, this.mySwarm.insectGroupData.Rad * (1f + Random.value))) && !crit.realizedCreature.slatedForDeletetion && crit.realizedCreature.room == this.room && !crit.creatureTemplate.smallCreature);
        }

        public class StaticElectricty : CosmeticSprite
        {
            public Color color;
            public Vector2 goalPos;
            public float inBetweenPoint;
            public float deviation;
            public float lifeTime;
            public float width;
            public float maxlifeTime;

            public StaticElectricty(Vector2 pos, Vector2 goalPos, float width, float lifeTime, Color color)
            {
                this.pos = pos;
                lastPos = pos;
                this.goalPos = goalPos;
                maxlifeTime = lifeTime;
                this.lifeTime = lifeTime;
                inBetweenPoint = Mathf.Lerp(0.2f, 0.8f, Random.value);
                deviation = Mathf.Lerp(-10f, 10f, Random.value);
                this.width = width;
                this.color = RandomizeColorABit(color, 0.05f, 0.1f);
            }

            public override void Update(bool eu)
            {
                base.Update(eu);

                lifeTime = Mathf.Max(0f, lifeTime - 0.1f);
                if (lifeTime == 0f)
                {
                    Destroy();
                }
            }

            public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                sLeaser.sprites = new FSprite[2];
                sLeaser.sprites[0] = new FSprite("pixel", true)
                {
                    anchorX = 0f,
                    anchorY = 0.5f,
                    scaleY = width,
                    shader = rCam.game.rainWorld.Shaders["Hologram"]
                };
                sLeaser.sprites[1] = new FSprite("pixel", true)
                {
                    anchorX = 0f,
                    anchorY = 0.5f,
                    scaleY = width,
                    shader = rCam.game.rainWorld.Shaders["Hologram"]
                };
                AddToContainer(sLeaser, rCam, null);
            }

            public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                Vector2 deviousPos = Vector2.Lerp(pos, goalPos, inBetweenPoint) + Custom.PerpendicularVector(pos, goalPos) * deviation;
                float alpf = Custom.LerpMap(lifeTime, maxlifeTime, 0f, 1f, 0f, 0.9f);

                sLeaser.sprites[0].scaleX = Vector2.Distance(pos, deviousPos);
                sLeaser.sprites[1].scaleX = Vector2.Distance(goalPos, deviousPos);
                sLeaser.sprites[0].scaleY = width * alpf;
                sLeaser.sprites[1].scaleY = width * alpf;
                sLeaser.sprites[0].SetPosition(pos - camPos);
                sLeaser.sprites[1].SetPosition(goalPos - camPos);
                sLeaser.sprites[0].rotation = Custom.VecToDeg(Custom.DirVec(pos, deviousPos)) - 90;
                sLeaser.sprites[1].rotation = Custom.VecToDeg(Custom.DirVec(goalPos, deviousPos)) - 90;
                sLeaser.sprites[0].alpha = alpf;
                sLeaser.sprites[1].alpha = alpf;

                base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            }

            public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
            {
                sLeaser.sprites[0].color = color;
                sLeaser.sprites[1].color = color;
            }
        }
    }
}
