using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Lunacy.CustomTokens
{
    // Copied from vanilla game and modified
    public class CustomCollectToken : UpdatableAndDeletable, IDrawable
    {
        public Vector2 hoverPos;
        public Vector2 pos;
        public Vector2 lastPos;
        public Vector2 vel;
        public float sinCounter;
        public float sinCounter2;
        public Vector2[] trail;
        public float expand;
        public float lastExpand;
        public bool contract;
        public Vector2[,] lines;
        public bool underWaterMode;
        public Player expandAroundPlayer;
        public float glitch;
        private float lastGlitch;
        private float generalGlitch;
        public PlacedObject placedObj;
        public CustomTokenStalk stalk;
        private bool poweredOn;
        public float power;
        public float lastPower;
        private StaticSoundLoop soundLoop;
        private StaticSoundLoop glitchLoop;
        public bool locked;
        private int lockdownCounter;
        public bool anythingUnlocked;
        public List<MultiplayerUnlocks.SandboxUnlockID> showUnlockSymbols;
        public CustomTokenDefinition definition;
        public Action<CustomCollectToken> finishCollectingCallback;

        public Color TokenColor
        {
            get
            {
                if (definition == null) return Color.white;
                return definition.tokenColor;
            }
        }

        public int LightSprite
        {
            get
            {
                return 0;
            }
        }

        public int MainSprite
        {
            get
            {
                return 1;
            }
        }

        public int TrailSprite
        {
            get
            {
                return 2;
            }
        }

        public int LineSprite(int line)
        {
            return 3 + line;
        }

        public int GoldSprite
        {
            get
            {
                return 7;
            }
        }

        public int TotalSprites
        {
            get
            {
                return 8;
            }
        }

        public CustomCollectToken(Room room, PlacedObject placedObj)
        {
            this.placedObj = placedObj;
            this.room = room;
            this.underWaterMode = (room.GetTilePosition(placedObj.pos).y < room.defaultWaterLevel);
            this.stalk = new CustomTokenStalk(room, placedObj.pos, placedObj.pos + (placedObj.data as CustomCollectTokenData).handlePos, this);
            room.AddObject(this.stalk);
            this.pos = placedObj.pos;
            this.hoverPos = this.pos;
            this.lastPos = this.pos;
            this.lines = new Vector2[4, 4];
            for (int i = 0; i < this.lines.GetLength(0); i++)
            {
                this.lines[i, 0] = this.pos;
                this.lines[i, 1] = this.pos;
            }
            this.lines[0, 2] = new Vector2(-7f, 0f);
            this.lines[1, 2] = new Vector2(0f, 11f);
            this.lines[2, 2] = new Vector2(7f, 0f);
            this.lines[3, 2] = new Vector2(0f, -11f);
            this.trail = new Vector2[5];
            for (int j = 0; j < this.trail.Length; j++)
            {
                this.trail[j] = this.pos;
            }
            this.soundLoop = new StaticSoundLoop(SoundID.Token_Idle_LOOP, this.pos, room, 0f, 1f);
            this.glitchLoop = new StaticSoundLoop(SoundID.Token_Upset_LOOP, this.pos, room, 0f, 1f);
            if (LunacyTokens.CustomTokenDefinitions.TryGetValue((placedObj.data as CustomCollectTokenData).customTokenID, out var def))
            {
                definition = def;
            }
            else Destroy();
        }

        public override void Update(bool eu)
        {
            if (ModManager.MMF && !this.AvailableToPlayer())
            {
                this.stalk.Destroy();
                this.Destroy();
            }
            this.sinCounter += UnityEngine.Random.value * this.power;
            this.sinCounter2 += (1f + Mathf.Lerp(-10f, 10f, UnityEngine.Random.value) * this.glitch) * this.power;
            float num = Mathf.Sin(this.sinCounter2 / 20f);
            num = Mathf.Pow(Mathf.Abs(num), 0.5f) * Mathf.Sign(num);
            this.soundLoop.Update();
            this.soundLoop.pos = this.pos;
            this.soundLoop.pitch = 1f + 0.25f * num * this.glitch;
            this.soundLoop.volume = Mathf.Pow(this.power, 0.5f) * Mathf.Pow(1f - this.glitch, 0.5f);
            this.glitchLoop.Update();
            this.glitchLoop.pos = this.pos;
            this.glitchLoop.pitch = Mathf.Lerp(0.75f, 1.25f, this.glitch) - 0.25f * num * this.glitch;
            this.glitchLoop.volume = Mathf.Pow(Mathf.Sin(Mathf.Clamp(this.glitch, 0f, 1f) * 3.1415927f), 0.1f) * Mathf.Pow(this.power, 0.1f);
            this.lastPos = this.pos;
            for (int i = 0; i < this.lines.GetLength(0); i++)
            {
                this.lines[i, 1] = this.lines[i, 0];
            }
            this.lastGlitch = this.glitch;
            this.lastExpand = this.expand;
            for (int j = this.trail.Length - 1; j >= 1; j--)
            {
                this.trail[j] = this.trail[j - 1];
            }
            this.trail[0] = this.lastPos;
            this.lastPower = this.power;
            this.power = Custom.LerpAndTick(this.power, this.poweredOn ? 1f : 0f, 0.07f, 0.025f);
            this.glitch = Mathf.Max(this.glitch, 1f - this.power);
            this.pos += this.vel;
            for (int k = 0; k < this.lines.GetLength(0); k++)
            {
                if (this.stalk != null)
                {
                    this.lines[k, 0] += this.stalk.head - this.stalk.lastHead;
                }
                if (Mathf.Pow(UnityEngine.Random.value, 0.1f + this.glitch * 5f) > this.lines[k, 3].x)
                {
                    this.lines[k, 0] = Vector2.Lerp(this.lines[k, 0], this.pos + new Vector2(this.lines[k, 2].x * num, this.lines[k, 2].y), Mathf.Pow(UnityEngine.Random.value, 1f + this.lines[k, 3].x * 17f));
                }
                if (UnityEngine.Random.value < Mathf.Pow(this.lines[k, 3].x, 0.2f) && UnityEngine.Random.value < Mathf.Pow(this.glitch, 0.8f - 0.4f * this.lines[k, 3].x))
                {
                    this.lines[k, 0] += Custom.RNV() * 17f * this.lines[k, 3].x * this.power;
                    this.lines[k, 3].y = Mathf.Max(this.lines[k, 3].y, this.glitch);
                }
                this.lines[k, 3].x = Custom.LerpAndTick(this.lines[k, 3].x, this.lines[k, 3].y, 0.01f, 0.033333335f);
                this.lines[k, 3].y = Mathf.Max(0f, this.lines[k, 3].y - 0.014285714f);
                if (UnityEngine.Random.value < 1f / Mathf.Lerp(210f, 20f, this.glitch))
                {
                    this.lines[k, 3].y = Mathf.Max(this.glitch, (UnityEngine.Random.value < 0.5f) ? this.generalGlitch : UnityEngine.Random.value);
                }
            }
            this.vel *= 0.995f;
            this.vel += Vector2.ClampMagnitude(this.hoverPos + new Vector2(0f, Mathf.Sin(this.sinCounter / 15f) * 7f) - this.pos, 15f) / 81f;
            this.vel += Custom.RNV() * UnityEngine.Random.value * UnityEngine.Random.value * Mathf.Lerp(0.06f, 0.4f, this.glitch);
            this.pos += Custom.RNV() * Mathf.Pow(UnityEngine.Random.value, 7f - 6f * this.generalGlitch) * Mathf.Lerp(0.06f, 1.2f, this.glitch);
            if (this.expandAroundPlayer != null)
            {
                this.expandAroundPlayer.Blink(5);
                if (!this.contract)
                {
                    this.expand += 0.033333335f;
                    if (this.expand > 1f)
                    {
                        this.expand = 1f;
                        this.contract = true;
                    }
                    this.generalGlitch = 0f;
                    this.glitch = Custom.LerpAndTick(this.glitch, this.expand * 0.5f, 0.07f, 0.06666667f);
                    float num2 = Custom.SCurve(Mathf.InverseLerp(0.35f, 0.55f, this.expand), 0.4f);
                    Vector2 b = Vector2.Lerp(this.expandAroundPlayer.mainBodyChunk.pos + new Vector2(0f, 40f), Vector2.Lerp(this.expandAroundPlayer.bodyChunks[1].pos, this.expandAroundPlayer.mainBodyChunk.pos + Custom.DirVec(this.expandAroundPlayer.bodyChunks[1].pos, this.expandAroundPlayer.mainBodyChunk.pos) * 10f, 0.65f), this.expand);
                    for (int l = 0; l < this.lines.GetLength(0); l++)
                    {
                        Vector2 b2 = Vector2.Lerp(this.lines[l, 2] * (2f + 5f * Mathf.Pow(this.expand, 0.5f)), Custom.RotateAroundOrigo(this.lines[l, 2] * (2f + 2f * Mathf.Pow(this.expand, 0.5f)), Custom.AimFromOneVectorToAnother(this.expandAroundPlayer.bodyChunks[1].pos, this.expandAroundPlayer.mainBodyChunk.pos)), num2);
                        this.lines[l, 0] = Vector2.Lerp(this.lines[l, 0], Vector2.Lerp(this.pos, b, Mathf.Pow(num2, 2f)) + b2, Mathf.Pow(this.expand, 0.5f));
                        this.lines[l, 3] *= 1f - this.expand;
                    }
                    this.hoverPos = Vector2.Lerp(this.hoverPos, b, Mathf.Pow(this.expand, 2f));
                    this.pos = Vector2.Lerp(this.pos, b, Mathf.Pow(this.expand, 2f));
                    this.vel *= 1f - this.expand;
                }
                else
                {
                    this.generalGlitch *= 1f - this.expand;
                    this.glitch = 0.15f;
                    this.expand -= 1f / Mathf.Lerp(60f, 2f, this.expand);
                    Vector2 a = Vector2.Lerp(this.expandAroundPlayer.bodyChunks[1].pos, this.expandAroundPlayer.mainBodyChunk.pos + Custom.DirVec(this.expandAroundPlayer.bodyChunks[1].pos, this.expandAroundPlayer.mainBodyChunk.pos) * 10f, Mathf.Lerp(1f, 0.65f, this.expand));
                    for (int m = 0; m < this.lines.GetLength(0); m++)
                    {
                        Vector2 b3 = Custom.RotateAroundOrigo(Vector2.Lerp((UnityEngine.Random.value > this.expand) ? this.lines[m, 2] : this.lines[UnityEngine.Random.Range(0, 4), 2], this.lines[UnityEngine.Random.Range(0, 4), 2], UnityEngine.Random.value * (1f - this.expand)) * (4f * Mathf.Pow(this.expand, 0.25f)), Custom.AimFromOneVectorToAnother(this.expandAroundPlayer.bodyChunks[1].pos, this.expandAroundPlayer.mainBodyChunk.pos)) * Mathf.Lerp(UnityEngine.Random.value, 1f, this.expand);
                        this.lines[m, 0] = a + b3;
                        this.lines[m, 3] *= 1f - this.expand;
                    }
                    this.pos = a;
                    this.hoverPos = a;
                    if (this.expand < 0f)
                    {
                        this.Destroy();
                        int num3 = 0;
                        while ((float)num3 < 20f)
                        {
                            this.room.AddObject(new CollectToken.TokenSpark(this.pos + Custom.RNV() * 2f, Custom.RNV() * 16f * UnityEngine.Random.value, Color.Lerp(this.TokenColor, new Color(1f, 1f, 1f), 0.5f + 0.5f * UnityEngine.Random.value), this.underWaterMode));
                            num3++;
                        }
                        this.room.PlaySound(SoundID.Token_Collected_Sparks, this.pos);
                        if (this.anythingUnlocked && this.room.game.cameras[0].hud != null && this.room.game.cameras[0].hud.textPrompt != null)
                        {
                            //else
                            //{
                            //    this.room.game.cameras[0].hud.textPrompt.AddMessage(this.room.game.manager.rainWorld.inGameTranslator.Translate("New arenas unlocked"), 20, 160, true, true);
                            //}
                            finishCollectingCallback.Invoke(this);
                        }
                    }
                }
            }
            else
            {
                this.generalGlitch = Mathf.Max(0f, this.generalGlitch - 0.008333334f);
                if (UnityEngine.Random.value < 0.0027027028f)
                {
                    this.generalGlitch = UnityEngine.Random.value;
                }
                if (!Custom.DistLess(this.pos, this.hoverPos, 11f))
                {
                    this.pos += Custom.DirVec(this.hoverPos, this.pos) * (11f - Vector2.Distance(this.pos, this.hoverPos)) * 0.7f;
                }
                float f = Mathf.Sin(Mathf.Clamp(this.glitch, 0f, 1f) * 3.1415927f);
                if (UnityEngine.Random.value < 0.05f + 0.35f * Mathf.Pow(f, 0.5f) && UnityEngine.Random.value < this.power)
                {
                    this.room.AddObject(new CollectToken.TokenSpark(this.pos + Custom.RNV() * 6f * this.glitch, Custom.RNV() * Mathf.Lerp(2f, 9f, Mathf.Pow(f, 2f)) * UnityEngine.Random.value, this.GoldCol(this.glitch), this.underWaterMode));
                }
                this.glitch = Custom.LerpAndTick(this.glitch, this.generalGlitch / 2f, 0.01f, 0.033333335f);
                if (UnityEngine.Random.value < 1f / Mathf.Lerp(360f, 10f, this.generalGlitch))
                {
                    this.glitch = Mathf.Pow(UnityEngine.Random.value, 1f - 0.85f * this.generalGlitch);
                }
                float num4 = float.MaxValue;
                bool flag = this.AvailableToPlayer();
                if (RainWorld.lockGameTimer)
                {
                    flag = false;
                }
                float num5 = 140f;
                for (int n = 0; n < this.room.game.session.Players.Count; n++)
                {
                    if (this.room.game.session.Players[n].realizedCreature != null && this.room.game.session.Players[n].realizedCreature.Consious && (this.room.game.session.Players[n].realizedCreature as Player).dangerGrasp == null && this.room.game.session.Players[n].realizedCreature.room == this.room)
                    {
                        num4 = Mathf.Min(num4, Vector2.Distance(this.room.game.session.Players[n].realizedCreature.mainBodyChunk.pos, this.pos));
                        if (flag)
                        {
                            if (Custom.DistLess(this.room.game.session.Players[n].realizedCreature.mainBodyChunk.pos, this.pos, 18f))
                            {
                                this.Pop(this.room.game.session.Players[n].realizedCreature as Player);
                                break;
                            }
                            if (Custom.DistLess(this.room.game.session.Players[n].realizedCreature.mainBodyChunk.pos, this.pos, num5))
                            {
                                if (Custom.DistLess(this.pos, this.hoverPos, 80f))
                                {
                                    this.pos += Custom.DirVec(this.pos, this.room.game.session.Players[n].realizedCreature.mainBodyChunk.pos) * Custom.LerpMap(Vector2.Distance(this.pos, this.room.game.session.Players[n].realizedCreature.mainBodyChunk.pos), 40f, num5, 2.2f, 0f, 0.5f) * UnityEngine.Random.value;
                                }
                                if (UnityEngine.Random.value < 0.05f && UnityEngine.Random.value < Mathf.InverseLerp(num5, 40f, Vector2.Distance(this.pos, this.room.game.session.Players[n].realizedCreature.mainBodyChunk.pos)))
                                {
                                    this.glitch = Mathf.Max(this.glitch, UnityEngine.Random.value * 0.5f);
                                }
                            }
                        }
                    }
                }
                if (!flag && this.poweredOn)
                {
                    this.lockdownCounter++;
                    if (UnityEngine.Random.value < 0.016666668f || num4 < num5 - 40f || this.lockdownCounter > 30)
                    {
                        this.locked = true;
                    }
                    if (UnityEngine.Random.value < 0.14285715f)
                    {
                        this.glitch = Mathf.Max(this.glitch, UnityEngine.Random.value * UnityEngine.Random.value * UnityEngine.Random.value);
                    }
                }
                if (this.poweredOn && (this.locked || (this.expand == 0f && !this.contract && UnityEngine.Random.value < Mathf.InverseLerp(num5 + 160f, num5 + 460f, num4))))
                {
                    this.poweredOn = false;
                    this.room.PlaySound(SoundID.Token_Turn_Off, this.pos);
                }
                else if (!this.poweredOn && !this.locked && UnityEngine.Random.value < Mathf.InverseLerp(num5 + 60f, num5 - 20f, num4))
                {
                    this.poweredOn = true;
                    this.room.PlaySound(SoundID.Token_Turn_On, this.pos);
                }
            }
            base.Update(eu);
        }

        private bool AvailableToPlayer()
        {
            return !(this.room.game.StoryCharacter == null) && (this.placedObj.data as CustomCollectTokenData).availableToPlayers.Contains(this.room.game.StoryCharacter);
        }

        public void Pop(Player player)
        {
            if (this.expand > 0f)
            {
                return;
            }
            this.expandAroundPlayer = player;
            this.expand = 0.01f;
            this.room.PlaySound(SoundID.Token_Collect, this.pos);
            finishCollectingCallback = definition.collectCallback.Invoke(player, this, (placedObj.data as CustomCollectTokenData).tokenString);
            int num = 0;
            while ((float)num < 10f)
            {
                this.room.AddObject(new CollectToken.TokenSpark(this.pos + Custom.RNV() * 2f, Custom.RNV() * 11f * UnityEngine.Random.value + Custom.DirVec(player.mainBodyChunk.pos, this.pos) * 5f * UnityEngine.Random.value, this.GoldCol(this.glitch), this.underWaterMode));
                num++;
            }
        }

        public Color GoldCol(float g)
        {
            return Color.Lerp(this.TokenColor, new Color(1f, 1f, 1f), 0.4f + 0.4f * Mathf.Max(this.contract ? 0.5f : (this.expand * 0.5f), Mathf.Pow(g, 0.5f)));
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[this.TotalSprites];
            sLeaser.sprites[this.LightSprite] = new FSprite("Futile_White", true);
            sLeaser.sprites[this.LightSprite].shader = rCam.game.rainWorld.Shaders[this.underWaterMode ? "UnderWaterLight" : "FlatLight"];
            sLeaser.sprites[this.GoldSprite] = new FSprite("Futile_White", true);
            sLeaser.sprites[this.GoldSprite].color = Color.Lerp(new Color(0f, 0f, 0f), RainWorld.GoldRGB, 0.2f);
            sLeaser.sprites[this.GoldSprite].shader = rCam.game.rainWorld.Shaders["FlatLight"];
            sLeaser.sprites[this.MainSprite] = new FSprite("JetFishEyeA", true);
            sLeaser.sprites[this.MainSprite].shader = rCam.game.rainWorld.Shaders["Hologram"];
            sLeaser.sprites[this.TrailSprite] = new FSprite("JetFishEyeA", true);
            sLeaser.sprites[this.TrailSprite].shader = rCam.game.rainWorld.Shaders["Hologram"];
            for (int i = 0; i < 4; i++)
            {
                sLeaser.sprites[this.LineSprite(i)] = new FSprite("pixel", true);
                sLeaser.sprites[this.LineSprite(i)].anchorY = 0f;
                sLeaser.sprites[this.LineSprite(i)].shader = rCam.game.rainWorld.Shaders["Hologram"];
            }
            this.AddToContainer(sLeaser, rCam, null);
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 vector = Vector2.Lerp(this.lastPos, this.pos, timeStacker);
            float num = Mathf.Lerp(this.lastGlitch, this.glitch, timeStacker);
            float num2 = Mathf.Lerp(this.lastExpand, this.expand, timeStacker);
            float num3 = Mathf.Lerp(this.lastPower, this.power, timeStacker);
            if (this.room != null && !this.AvailableToPlayer())
            {
                num = Mathf.Lerp(num, 1f, UnityEngine.Random.value);
                num3 *= 0.3f + 0.7f * UnityEngine.Random.value;
            }
            sLeaser.sprites[this.GoldSprite].x = vector.x - camPos.x;
            sLeaser.sprites[this.GoldSprite].y = vector.y - camPos.y;
            sLeaser.sprites[this.GoldSprite].alpha = 0.75f * Mathf.Lerp(Mathf.Lerp(0.8f, 0.5f, Mathf.Pow(num, 0.6f + 0.2f * UnityEngine.Random.value)), 0.7f, num2) * num3;
            sLeaser.sprites[this.GoldSprite].scale = Mathf.Lerp(100f, 300f, num2) / 16f;
            Color color = this.GoldCol(num);
            sLeaser.sprites[this.MainSprite].color = color;
            sLeaser.sprites[this.MainSprite].x = vector.x - camPos.x;
            sLeaser.sprites[this.MainSprite].y = vector.y - camPos.y;
            sLeaser.sprites[this.MainSprite].alpha = (1f - num) * Mathf.InverseLerp(0.5f, 0f, num2) * num3 * (this.underWaterMode ? 0.5f : 1f);
            sLeaser.sprites[this.MainSprite].isVisible = (!this.contract && num3 > 0f);
            sLeaser.sprites[this.TrailSprite].color = color;
            sLeaser.sprites[this.TrailSprite].x = Mathf.Lerp(this.trail[this.trail.Length - 1].x, this.trail[this.trail.Length - 2].x, timeStacker) - camPos.x;
            sLeaser.sprites[this.TrailSprite].y = Mathf.Lerp(this.trail[this.trail.Length - 1].y, this.trail[this.trail.Length - 2].y, timeStacker) - camPos.y;
            sLeaser.sprites[this.TrailSprite].alpha = 0.75f * (1f - num) * Mathf.InverseLerp(0.5f, 0f, num2) * num3 * (this.underWaterMode ? 0.5f : 1f);
            sLeaser.sprites[this.TrailSprite].isVisible = (!this.contract && num3 > 0f);
            sLeaser.sprites[this.TrailSprite].scaleX = ((UnityEngine.Random.value < num) ? (1f + 20f * UnityEngine.Random.value * this.glitch) : 1f);
            sLeaser.sprites[this.TrailSprite].scaleY = ((UnityEngine.Random.value < num) ? (1f + 2f * UnityEngine.Random.value * UnityEngine.Random.value * this.glitch) : 1f);
            sLeaser.sprites[this.LightSprite].x = vector.x - camPos.x;
            sLeaser.sprites[this.LightSprite].y = vector.y - camPos.y;
            if (this.underWaterMode)
            {
                sLeaser.sprites[this.LightSprite].alpha = Mathf.Pow(0.9f * (1f - num) * Mathf.InverseLerp(0.5f, 0f, num2) * num3, 0.5f);
                sLeaser.sprites[this.LightSprite].scale = Mathf.Lerp(60f, 120f, num) / 16f;
            }
            else
            {
                sLeaser.sprites[this.LightSprite].alpha = 0.9f * (1f - num) * Mathf.InverseLerp(0.5f, 0f, num2) * num3;
                sLeaser.sprites[this.LightSprite].scale = Mathf.Lerp(20f, 40f, num) / 16f;
            }
            sLeaser.sprites[this.LightSprite].color = Color.Lerp(this.TokenColor, color, 0.4f);
            sLeaser.sprites[this.LightSprite].isVisible = (!this.contract && num3 > 0f);
            for (int i = 0; i < 4; i++)
            {
                Vector2 vector2 = Vector2.Lerp(this.lines[i, 1], this.lines[i, 0], timeStacker);
                int num4 = (i == 3) ? 0 : (i + 1);
                Vector2 vector3 = Vector2.Lerp(this.lines[num4, 1], this.lines[num4, 0], timeStacker);
                float num5 = 1f - (1f - Mathf.Max(this.lines[i, 3].x, this.lines[num4, 3].x)) * (1f - num);
                num5 = Mathf.Pow(num5, 2f);
                num5 *= 1f - num2;
                if (UnityEngine.Random.value < num5)
                {
                    vector3 = Vector2.Lerp(vector2, vector3, UnityEngine.Random.value);
                    if (this.stalk != null)
                    {
                        vector2 = this.stalk.EyePos(timeStacker);
                    }
                    if (this.expandAroundPlayer != null && (UnityEngine.Random.value < this.expand || this.contract))
                    {
                        vector2 = Vector2.Lerp(this.expandAroundPlayer.mainBodyChunk.lastPos, this.expandAroundPlayer.mainBodyChunk.pos, timeStacker);
                    }
                }
                sLeaser.sprites[this.LineSprite(i)].x = vector2.x - camPos.x;
                sLeaser.sprites[this.LineSprite(i)].y = vector2.y - camPos.y;
                sLeaser.sprites[this.LineSprite(i)].scaleY = Vector2.Distance(vector2, vector3);
                sLeaser.sprites[this.LineSprite(i)].rotation = Custom.AimFromOneVectorToAnother(vector2, vector3);
                sLeaser.sprites[this.LineSprite(i)].alpha = (1f - num5) * num3 * (this.underWaterMode ? 0.2f : 1f);
                sLeaser.sprites[this.LineSprite(i)].color = color;
                sLeaser.sprites[this.LineSprite(i)].isVisible = (num3 > 0f);
            }
            if (base.slatedForDeletetion || this.room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (newContatiner == null)
            {
                newContatiner = rCam.ReturnFContainer("Water");
            }
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].RemoveFromContainer();
            }
            newContatiner.AddChild(sLeaser.sprites[this.GoldSprite]);
            for (int j = 0; j < this.GoldSprite; j++)
            {
                bool flag = false;
                if (ModManager.MMF)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        if (j == this.LineSprite(k))
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                if (!flag)
                {
                    newContatiner.AddChild(sLeaser.sprites[j]);
                }
            }
            if (ModManager.MMF)
            {
                for (int l = 0; l < 4; l++)
                {
                    rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[this.LineSprite(l)]);
                }
            }
        }
    }
}
