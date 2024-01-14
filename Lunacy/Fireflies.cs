using UnityEngine;
using RWCustom;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System.Reflection;
using System.Collections.Generic;
using System;
using MoreSlugcats;
using System.Runtime.CompilerServices;
using System.Linq;

namespace Lunacy
{
    public class Fireflies
    {
        public static ConditionalWeakTable<AbstractCreature, FliedFireFly> FiredFly = new ();

        public static void Apply()
        {
            // Initializing when the flies become fireflies
            On.FliesRoomAI.CreateFlyInHive += FliesRoomAI_CreateFlyInHive;
            IL.Room.PlaceQuantifiedCreaturesInRoom += Room_PlaceQuantifiedCreaturesInRoomIL;
            IL.World.MoveQuantifiedCreatureFromAbstractRoom += World_MoveQuantifiedCreatureFromAbstractRoomIL;

            // Firefly graphics
            On.FlyGraphics.InitiateSprites += FlyGraphics_InitiateSprites;
            On.FlyGraphics.DrawSprites += FlyGraphics_DrawSprites;
            On.FlyGraphics.ApplyPalette += FlyGraphics_ApplyPalette;
            On.FlyGraphics.AddToContainer += FlyGraphics_AddToContainer;

            On.Fly.BitByPlayer += Fly_BitByPlayer;
        }

        public static void World_MoveQuantifiedCreatureFromAbstractRoomIL(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchNewobj<AbstractCreature>()
                ))
            {
                c.Emit(OpCodes.Ldarg_2);
                c.EmitDelegate<Func<AbstractCreature, AbstractRoom, AbstractCreature>>((orig, room) =>
                {
                    if (room.world.fliesWorldAI != null)
                    {
                        try
                        {
                            List<int> randoe = room.world.swarmRooms.Where(x => room.world.GetSwarmRoom(x).roomTags.Any(z => z.StartsWith("FIREFLIES"))).ToList();
                            foreach (var g in randoe) Plugin.logger.LogMessage(g);
                            Plugin.logger.LogMessage("-");
                            foreach (var s in room.world.swarmRooms) Plugin.logger.LogMessage(s);
                            float r = randoe.Count / room.world.swarmRooms.Length;
                            if (UnityEngine.Random.value > r)
                            {
                                string f = room.world.abstractRooms[randoe[UnityEngine.Random.Range(0, randoe.Count)]].roomTags.Find(x => x.StartsWith("FIREFLIES"));
                                MakeFlyFireEmoji(room.realizedRoom, orig, f);
                            }
                        }
                        catch(Exception e)
                        {
                            Plugin.logger.LogError(e);
                        }
                    }
                    return orig;
                });
            }
            else Plugin.logger.LogMessage("World_MoveQuantifiedCreatureFromAbstractRoom FAILED " + il);
        }

        public static void Room_PlaceQuantifiedCreaturesInRoomIL(ILContext il)
        {
            ILCursor c = new (il);

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<AbstractPhysicalObject>("RealizeInRoom")
                ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc, 15);
                c.EmitDelegate<Action<Room, AbstractCreature>>((room, crit) =>
                {
                    MakeFlyFireEmoji(room, crit, room.abstractRoom.roomTags.Find(x => x.StartsWith("FIREFLIES")));
                });
            }
            else Plugin.logger.LogMessage("Room_PlaceQuantifiedCreaturesInRoomIL FAILED " + il);
        }

        public static void FliesRoomAI_CreateFlyInHive(On.FliesRoomAI.orig_CreateFlyInHive orig, FliesRoomAI self)
        {
            orig.Invoke(self);
            MakeFlyFireEmoji(self.room, self.inHive[self.inHive.Count - 1].abstractCreature, self.room.abstractRoom.roomTags.Find(x => x.StartsWith("FIREFLIES")));
        }

        public static void MakeFlyFireEmoji(Room room, AbstractCreature c, string fireString)
        {
            if (c.creatureTemplate.type != CreatureTemplate.Type.Fly) return;
            if (!FiredFly.TryGetValue(c, out _))
            {
                if (fireString == null) return;
                string[] colorString = fireString.Split(['|']);

                FliedFireFly thinj = null;
                float hue = 68f;
                if (colorString.Length > 1)
                {
                    if (!float.TryParse(colorString[1], out hue))
                    {
                        if (colorString[1] != "A" && colorString[1] != "B")
                        {
                            throw new($"Incorrect effect color declaration in {colorString[0]}|{colorString[1]}!!!");
                        }
                        else
                        {
                            Vector3 ec = Custom.RGB2HSL(RoomCamera.allEffectColorsTexture.GetPixel((colorString[1] == "A" ? room.roomSettings.EffectColorA : room.roomSettings.EffectColorB) * 2, 0));

                            thinj = new FliedFireFly(c, Custom.HSL2RGB(ec.x, ec.y * 1.8f, ec.z));
                            goto skippy;
                        }
                    }
                }
                thinj = new FliedFireFly(c, Mathf.Clamp(hue, 0f, 360f) / 360f);
            skippy:
                FiredFly.Add(c, thinj);
                Plugin.logger.LogMessage("Turning guy into fly");
            }
        }

        public static void FlyGraphics_InitiateSprites(On.FlyGraphics.orig_InitiateSprites orig, FlyGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig.Invoke(self, sLeaser, rCam);
            if (FiredFly.TryGetValue(self.fly.abstractCreature, out var fire)) { fire.InitiateSprites(sLeaser, rCam); self.AddToContainer(sLeaser, rCam, null); }
        }

        public static void FlyGraphics_DrawSprites(On.FlyGraphics.orig_DrawSprites orig, FlyGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig.Invoke(self, sLeaser, rCam, timeStacker, camPos);
            if (FiredFly.TryGetValue(self.fly.abstractCreature, out var fire)) fire.DrawUpdate(self, sLeaser, rCam, timeStacker, camPos);
        }

        public static void FlyGraphics_ApplyPalette(On.FlyGraphics.orig_ApplyPalette orig, FlyGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig.Invoke(self, sLeaser, rCam, palette);
            if (FiredFly.TryGetValue(self.fly.abstractCreature, out var fire)) fire.ApplyPalette(sLeaser, rCam, palette);
        }

        public static void FlyGraphics_AddToContainer(On.FlyGraphics.orig_AddToContainer orig, FlyGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig.Invoke(self, sLeaser, rCam, newContatiner);
            if (FiredFly.TryGetValue(self.fly.abstractCreature, out var fire)) fire.AddToContainer(sLeaser, rCam);
        }

        public static void Fly_BitByPlayer(On.Fly.orig_BitByPlayer orig, Fly self, Creature.Grasp grasp, bool eu)
        {
            orig.Invoke(self, grasp, eu);

            if (FiredFly.TryGetValue(self.abstractCreature, out var fire) && self.eaten == 3) 
            {
                self.room.PlaySound(SoundID.Spore_Bee_Spark, self.mainBodyChunk.pos, 1f, 2f);
                self.room.AddObject(new Spark(self.mainBodyChunk.pos, Custom.RNV() * 5f, fire.color, null, 7, 14));
                self.room.AddObject(new Explosion.ExplosionLight(self.mainBodyChunk.pos, 50f * fire.scaleFac, 1f, 4, fire.color));
            }
        }

        // How's Fly gonna get a job now :((
        public class FliedFireFly
        {
            public AbstractCreature flyAbstract;
            public Fly fly => flyAbstract.realizedCreature as Fly;
            public LightSource light;
            public int newSpriteIndex;
            public readonly int totalSprites = 3;
            public Color color;
            public float scaleFac;

            public FliedFireFly(AbstractCreature fly, float hue) : this(fly, Custom.HSL2RGB(hue, 0.91f, 0.5f)) { }
            public FliedFireFly(AbstractCreature fly, Color color)
            {
                flyAbstract = fly;
                this.color = color;
                // So each fly has smth a lil different :)) (all flies are personalized)
                scaleFac = Mathf.Lerp(0.8f, 1.2f, fly.world.game.SeededRandom(fly.ID.RandomSeed));
            }

            public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                newSpriteIndex = sLeaser.sprites.Length;
                Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + totalSprites);

                sLeaser.sprites[newSpriteIndex] = new FSprite("FlyLight");
                sLeaser.sprites[newSpriteIndex + 1] = new FSprite("Futile_White", true)
                {
                    shader = rCam.room.game.rainWorld.Shaders["FlatLight"],
                    scale = 1.5f * scaleFac,
                };
                sLeaser.sprites[newSpriteIndex + 2] = new FSprite("Futile_White", true)
                {
                    shader = rCam.room.game.rainWorld.Shaders["LightSource"],
                    scale = 10f * scaleFac,
                };
            }

            public void DrawUpdate(FlyGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                sLeaser.sprites[newSpriteIndex].SetPosition(sLeaser.sprites[0].GetPosition());
                sLeaser.sprites[newSpriteIndex].rotation = sLeaser.sprites[0].rotation;

                // Light
                Vector2 buttPos = Vector2.Lerp(self.lowerBody.lastPos, self.lowerBody.pos, timeStacker);
                Vector2 bodyPos = Vector2.Lerp(self.fly.bodyChunks[0].lastPos, self.fly.bodyChunks[0].pos, timeStacker);
                Vector2 lightPos = bodyPos + Custom.DirVec(bodyPos, buttPos) * 4f;
                sLeaser.sprites[newSpriteIndex + 1].SetPosition(lightPos - camPos);
                sLeaser.sprites[newSpriteIndex + 1].rotation = sLeaser.sprites[0].rotation;
                sLeaser.sprites[newSpriteIndex + 1].alpha = 0.1f + 0.2f * rCam.room.Darkness(self.fly.bodyChunks[0].pos);

                sLeaser.sprites[newSpriteIndex + 2].SetPosition(lightPos - camPos);
                sLeaser.sprites[newSpriteIndex + 2].rotation = sLeaser.sprites[0].rotation;
                sLeaser.sprites[newSpriteIndex + 2].alpha = 0.7f + 0.3f * rCam.room.Darkness(self.fly.bodyChunks[0].pos);
            }

            public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
            {
                sLeaser.sprites[newSpriteIndex].color = color;
                sLeaser.sprites[newSpriteIndex + 1].color = color;
                sLeaser.sprites[newSpriteIndex + 2].color = color;
            }

            public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                if (sLeaser.sprites.Length > newSpriteIndex)
                {
                    var mid = rCam.ReturnFContainer("Midground");
                    mid.RemoveChild(sLeaser.sprites[newSpriteIndex + 1]);
                    mid.RemoveChild(sLeaser.sprites[newSpriteIndex + 2]);
                    rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[newSpriteIndex + 1]);
                    rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[newSpriteIndex + 2]);
                }
            }
        }
    }
}
