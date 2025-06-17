using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Lunacy
{
    public class Fireflies
    {
        public static ConditionalWeakTable<AbstractCreature, FliedFireFly> FiredFly = new ();
        public static Dictionary<string, int> AbstractRoomEffectA = new Dictionary<string, int>();
        public static Dictionary<string, int> AbstractRoomEffectB = new Dictionary<string, int>();

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

            // Eated effect
            On.Fly.BitByPlayer += Fly_BitByPlayer;

            // Fading after death
            On.Fly.Update += Fly_Update;

            // CRS please fix yourself
            IL.World.LoadMapConfig_Timeline += World_LoadMapConfig;

            // Thing i dont yeah thing.
            On.World.LoadWorld_Timeline_List1_Int32Array_Int32Array_Int32Array += World_LoadWorld;
        }

        private static void World_LoadWorld(On.World.orig_LoadWorld_Timeline_List1_Int32Array_Int32Array_Int32Array orig, World self, SlugcatStats.Timeline timeline, List<AbstractRoom> abstractRoomsList, int[] swarmRooms, int[] shelters, int[] gates)
        {
            orig.Invoke(self, timeline, abstractRoomsList, swarmRooms, shelters, gates);

            AbstractRoomEffectA.Clear();
            AbstractRoomEffectB.Clear();
            if (swarmRooms == null) return;
            for (int i = 0; i < swarmRooms.Length; i++)
            {
                AbstractRoom room = self.GetSwarmRoom(i);
                if (room.roomTags == null) continue;
                if (room.roomTags.Any(x => x.StartsWith("FIREFLIES")))
                {
                    RoomSettings tempSettings = new(room.name, self.region, false, false, timeline, self.game);
                    AbstractRoomEffectA[room.name] = tempSettings.EffectColorA;
                    AbstractRoomEffectB[room.name] = tempSettings.EffectColorB;
                    tempSettings = null;
                }
            }
        }

        public static void World_LoadMapConfig(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchStloc(14)
                ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc, 14);
                c.EmitDelegate<Action<World, string[]>>((world, line) =>
                {
                    if (line[0] == "Fireflies")
                    {
                        string[] rooms = Regex.Split(line[1], ", ");
                        foreach (var room in rooms)
                        {
                            string[] gruh = room.Split('|');
                            for (int i = 0; i < world.NumberOfRooms; i++)
                            {
                                if (world.abstractRooms[i].name == gruh[0] && (world.abstractRooms[i].roomTags == null || !world.abstractRooms[i].roomTags.Any(x => x.StartsWith("FIREFLIES"))))
                                {
                                    string grug = "";
                                    if (gruh.Length > 1)
                                    {
                                        grug += "|" + gruh[1];
                                    }
                                    world.abstractRooms[i].AddTag("FIREFLIES" + grug);
                                    Plugin.logger.LogMessage($"Added room {room} as a firefly spot wahoo - {"FIREFLIES" + grug}");
                                }
                            }
                        }
                    }

                    // mt lightning
                    else if (line[0] == "LightningBgColorA")
                    {
                        string[] ints = Regex.Split(line[1].Trim(), " ");
                        Color c = new(0.19607843f, 0.23529412f, 0.78431374f);
                        for (int i = 0; i < Mathf.Min(3, ints.Length); i++)
                        {
                            if (!int.TryParse(ints[i], out int v))
                            {
                                Plugin.logger.LogError("Failed to parse int lightning bg color A, supplying default value");
                                continue;
                            }
                            c[i] = Mathf.Abs(v) / 255f;
                        }
                        if (!Metropolis.lightningGradientColors.ContainsKey(world.name)) Metropolis.lightningGradientColors.Add(world.name, new Color[2]);
                        Metropolis.lightningGradientColors[world.name][0] = c;
                    }
                    else if (line[0] == "LightningBgColorB")
                    {
                        string[] ints = Regex.Split(line[1].Trim(), " ");
                        Color c = new(0.21176471f, 1f, 0.22352941f);
                        for (int i = 0; i < Mathf.Min(3, ints.Length); i++)
                        {
                            if (!int.TryParse(ints[i], out int v))
                            {
                                Plugin.logger.LogError("Failed to parse int lightning bg color B, supplying default value");
                                continue;
                            }
                            c[i] = Mathf.Abs(v) / 255f;
                        }
                        if (!Metropolis.lightningGradientColors.ContainsKey(world.name)) Metropolis.lightningGradientColors.Add(world.name, new Color[2]);
                        Metropolis.lightningGradientColors[world.name][1] = c;
                    }
                });
            }
            else Plugin.logger.LogError("World_LoadMapConfig FAILED " + il);
        }

        public static void Fly_Update(On.Fly.orig_Update orig, Fly self, bool eu)
        {
            orig.Invoke(self, eu);

            if (self.dead && FiredFly.TryGetValue(self.abstractCreature, out var fire)) fire.DeathUpdate();
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
                    try
                    {
                        if (room.world.swarmRooms == null || room.world.swarmRooms.Length == 0) return orig;
                        List<int> raro = [];
                        for (int i = 0; i < room.world.swarmRooms.Length; i++)
                        {
                            if (room.world.GetSwarmRoom(i).roomTags == null) continue;
                            if (room.world.GetSwarmRoom(i).roomTags.Any(x => x.StartsWith("FIREFLIES"))) raro.Add(room.world.swarmRooms[i]);
                        }

                        float r = raro.Count / room.world.swarmRooms.Length;
                        if (UnityEngine.Random.value < r)
                        {
                            AbstractRoom rome = room.world.GetAbstractRoom(raro[UnityEngine.Random.Range(0, raro.Count)]);
                            string f = rome.roomTags.Find(x => x.StartsWith("FIREFLIES"));
                            MakeFlyFireEmoji(rome.name, orig, f);
                        }
                    }
                    catch (Exception e)
                    {
                        Plugin.logger.LogError(e);
                    }
                    return orig;
                });
            }
            else Plugin.logger.LogError("World_MoveQuantifiedCreatureFromAbstractRoom FAILED " + il);
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
                    if (room.abstractRoom.roomTags != null) MakeFlyFireEmoji(room.abstractRoom.name, crit, room.abstractRoom.roomTags.Find(x => x.StartsWith("FIREFLIES")));
                });
            }
            else Plugin.logger.LogError("Room_PlaceQuantifiedCreaturesInRoomIL FAILED " + il);
        }

        public static void FliesRoomAI_CreateFlyInHive(On.FliesRoomAI.orig_CreateFlyInHive orig, FliesRoomAI self)
        {
            orig.Invoke(self);
            if (self.room.abstractRoom.roomTags != null) MakeFlyFireEmoji(self.room.abstractRoom.name, self.inHive[self.inHive.Count - 1].abstractCreature, self.room.abstractRoom.roomTags.Find(x => x.StartsWith("FIREFLIES")));
        }

        public static void MakeFlyFireEmoji(string roomName, AbstractCreature c, string fireString)
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
                            Vector3 ec = Custom.RGB2HSL(RoomCamera.allEffectColorsTexture.GetPixel((colorString[1] == "A" ? AbstractRoomEffectA[roomName] : AbstractRoomEffectB[roomName]) * 2, 0));

                            thinj = new FliedFireFly(c, Custom.HSL2RGB(ec.x, ec.y * 1.8f, ec.z));
                            goto skippy;
                        }
                    }
                }
                thinj = new FliedFireFly(c, Mathf.Clamp(hue, 0f, 360f) / 360f);
            skippy:
                FiredFly.Add(c, thinj);
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
                self.room.AddObject(new Explosion.ExplosionLight(self.mainBodyChunk.pos, 50f * fire.scaleFac, 1f, 7, fire.color));
                if (UnityEngine.Random.value < 0.1f) self.room.AddObject(new CreatureSpasmer(grasp.grabber, false, 60));
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
            public float lastFade;
            public float fade;

            public FliedFireFly(AbstractCreature fly, float hue) : this(fly, Custom.HSL2RGB(hue, 0.91f, 0.5f)) { }
            public FliedFireFly(AbstractCreature fly, Color color)
            {
                flyAbstract = fly;
                this.color = color;
                // So each fly has smth a lil different :)) (all flies are personalized)
                scaleFac = Mathf.Lerp(0.8f, 1.2f, fly.world.game.SeededRandom(fly.ID.RandomSeed));
                fade = 1f;
                lastFade = fade;
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

            public void DeathUpdate()
            {
                lastFade = fade;
                fade = Mathf.Max(0f, fade - Mathf.Lerp(0.00005f, 0.0005f, UnityEngine.Random.value));
            }

            public void DrawUpdate(FlyGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                sLeaser.sprites[newSpriteIndex].SetPosition(sLeaser.sprites[0].GetPosition());
                sLeaser.sprites[newSpriteIndex].rotation = sLeaser.sprites[0].rotation;

                // Light
                Vector2 buttPos = Vector2.Lerp(self.lowerBody.lastPos, self.lowerBody.pos, timeStacker);
                Vector2 bodyPos = Vector2.Lerp(self.fly.bodyChunks[0].lastPos, self.fly.bodyChunks[0].pos, timeStacker);
                Vector2 lightPos = bodyPos + Custom.DirVec(bodyPos, buttPos) * 4f;
                float scaleFacc = Mathf.Lerp(lastFade, fade, timeStacker);

                sLeaser.sprites[newSpriteIndex + 1].SetPosition(lightPos - camPos);
                sLeaser.sprites[newSpriteIndex + 1].rotation = sLeaser.sprites[0].rotation;
                sLeaser.sprites[newSpriteIndex + 1].alpha = 0.1f + 0.3f * rCam.room.Darkness(self.fly.bodyChunks[0].pos);

                sLeaser.sprites[newSpriteIndex + 2].SetPosition(lightPos - camPos);
                sLeaser.sprites[newSpriteIndex + 2].rotation = sLeaser.sprites[0].rotation;
                sLeaser.sprites[newSpriteIndex + 2].alpha = 0.7f + 0.2f * rCam.room.Darkness(self.fly.bodyChunks[0].pos);

                // Fading
                if (scaleFacc < 1f)
                {
                    sLeaser.sprites[newSpriteIndex + 1].scale = 1.5f * scaleFac * scaleFacc;
                    sLeaser.sprites[newSpriteIndex + 2].scale = 10f * scaleFac * scaleFacc;
                }
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
