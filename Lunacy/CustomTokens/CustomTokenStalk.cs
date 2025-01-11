using System;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Lunacy.CustomTokens
{
    // Copied from vanilla game and modified
    public class CustomTokenStalk : UpdatableAndDeletable, IDrawable
    {
        public Vector2 hoverPos;
        public CustomCollectToken token;
        public Vector2[,] stalk;
        public Vector2 basePos;
        public Vector2 mainDir;
        public float flip;
        public Vector2 armPos;
        public Vector2 lastArmPos;
        public Vector2 armVel;
        public Vector2 armGetToPos;
        public Vector2 head;
        public Vector2 lastHead;
        public Vector2 headVel;
        public Vector2 headDir;
        public Vector2 lastHeadDir;
        private float headDist = 15f;
        public float armLength;
        private Vector2[,] coord;
        private float coordLength;
        private float coordSeg = 3f;
        private float[,] curveLerps;
        private float keepDistance;
        private float sinCounter;
        private float lastSinCounter;
        private float lampPower;
        private float lastLampPower;
        private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;
        public Color lampColor;
        public bool forceSatellite;
        public int sataFlasherLight;
        private Color lampOffCol;

        public int BaseSprite
        {
            get
            {
                return 0;
            }
        }

        public int Arm1Sprite
        {
            get
            {
                return 1;
            }
        }

        public int Arm2Sprite
        {
            get
            {
                return 2;
            }
        }

        public int Arm3Sprite
        {
            get
            {
                return 3;
            }
        }

        public int Arm4Sprite
        {
            get
            {
                return 4;
            }
        }

        public int Arm5Sprite
        {
            get
            {
                return 5;
            }
        }

        public int ArmJointSprite
        {
            get
            {
                return 6;
            }
        }

        public int SocketSprite
        {
            get
            {
                return 7;
            }
        }

        public int HeadSprite
        {
            get
            {
                return 8;
            }
        }

        public int LampSprite
        {
            get
            {
                return 9;
            }
        }

        public int SataFlasher
        {
            get
            {
                return 10;
            }
        }

        public int CoordSprite(int s)
        {
            return (ModManager.MSC ? 11 : 10) + s;
        }

        public int TotalSprites
        {
            get
            {
                return (ModManager.MSC ? 11 : 10) + this.coord.GetLength(0);
            }
        }

        public float alive
        {
            get
            {
                if (this.token == null)
                {
                    return 0f;
                }
                return 0.25f + 0.75f * this.token.power;
            }
        }

        public CustomTokenStalk(Room room, Vector2 hoverPos, Vector2 basePos, CustomCollectToken token)
        {
            this.token = token;
            this.hoverPos = hoverPos;
            this.basePos = basePos;
            if (token != null)
            {
                this.lampPower = 1f;
                this.lastLampPower = 1f;
            }
            this.lampColor = Color.Lerp(RainWorld.GoldRGB, new Color(1f, 1f, 1f), 0.5f);
            UnityEngine.Random.State state = UnityEngine.Random.state;
            UnityEngine.Random.InitState((int)(hoverPos.x * 10f) + (int)(hoverPos.y * 10f));
            this.curveLerps = new float[2, 5];
            for (int i = 0; i < this.curveLerps.GetLength(0); i++)
            {
                this.curveLerps[i, 0] = 1f;
                this.curveLerps[i, 1] = 1f;
            }
            this.curveLerps[0, 3] = UnityEngine.Random.value * 360f;
            this.curveLerps[1, 3] = Mathf.Lerp(10f, 20f, UnityEngine.Random.value);
            this.flip = ((UnityEngine.Random.value < 0.5f) ? -1f : 1f);
            this.mainDir = Custom.DirVec(basePos, hoverPos);
            this.coordLength = Vector2.Distance(basePos, hoverPos) * 0.6f;
            this.coord = new Vector2[(int)(this.coordLength / this.coordSeg), 3];
            this.armLength = Vector2.Distance(basePos, hoverPos) / 2f;
            this.armPos = basePos + this.mainDir * this.armLength;
            this.lastArmPos = this.armPos;
            this.armGetToPos = this.armPos;
            for (int j = 0; j < this.coord.GetLength(0); j++)
            {
                this.coord[j, 0] = this.armPos;
                this.coord[j, 1] = this.armPos;
            }
            this.head = hoverPos - this.mainDir * this.headDist;
            this.lastHead = this.head;
            UnityEngine.Random.state = state;
        }

        public override void Update(bool eu)
        {
            this.lastArmPos = this.armPos;
            this.armPos += this.armVel;
            this.armPos = Custom.MoveTowards(this.armPos, this.armGetToPos, (0.8f + this.armLength / 150f) / 2f);
            this.armVel *= 0.8f;
            this.armVel += Vector2.ClampMagnitude(this.armGetToPos - this.armPos, 4f) / 11f;
            this.lastHead = this.head;
            this.head += this.headVel;
            this.headVel *= 0.8f;
            if (this.token != null && this.token.slatedForDeletetion)
            {
                this.token = null;
            }
            this.lastLampPower = this.lampPower;
            this.lastSinCounter = this.sinCounter;
            this.sinCounter += UnityEngine.Random.value * this.lampPower;
            if (this.token != null)
            {
                this.lampPower = Custom.LerpAndTick(this.lampPower, 1f, 0.02f, 0.016666668f);
            }
            else
            {
                this.lampPower = Mathf.Max(0f, this.lampPower - 0.008333334f);
            }
            if (!Custom.DistLess(this.head, this.armPos, this.coordLength))
            {
                this.headVel -= Custom.DirVec(this.armPos, this.head) * (Vector2.Distance(this.armPos, this.head) - this.coordLength) * 0.8f;
                this.head -= Custom.DirVec(this.armPos, this.head) * (Vector2.Distance(this.armPos, this.head) - this.coordLength) * 0.8f;
            }
            this.headVel += (Vector2)Vector3.Slerp(Custom.DegToVec(this.GetCurveLerp(0, 0.5f, 1f)), new Vector2(0f, 1f), 0.4f) * 0.4f;
            this.lastHeadDir = this.headDir;
            Vector2 vector = this.hoverPos;
            if (this.token != null && this.token.expand == 0f && !this.token.contract)
            {
                vector = Vector2.Lerp(this.hoverPos, this.token.pos, this.alive);
            }
            this.headVel -= Custom.DirVec(vector, this.head) * (Vector2.Distance(vector, this.head) - this.headDist) * 0.8f;
            this.head -= Custom.DirVec(vector, this.head) * (Vector2.Distance(vector, this.head) - this.headDist) * 0.8f;
            this.headDir = Custom.DirVec(this.head, vector);
            if (UnityEngine.Random.value < 1f / Mathf.Lerp(300f, 60f, this.alive))
            {
                Vector2 b = this.basePos + this.mainDir * this.armLength * 0.7f + Custom.RNV() * UnityEngine.Random.value * this.armLength * Mathf.Lerp(0.1f, 0.3f, this.alive);
                if (SharedPhysics.RayTraceTilesForTerrain(this.room, this.armGetToPos, b))
                {
                    this.armGetToPos = b;
                }
                this.NewCurveLerp(0, this.curveLerps[0, 3] + Mathf.Lerp(-180f, 180f, UnityEngine.Random.value), Mathf.Lerp(1f, 2f, this.alive));
                this.NewCurveLerp(1, Mathf.Lerp(10f, 20f, Mathf.Pow(UnityEngine.Random.value, 0.75f)), Mathf.Lerp(0.4f, 0.8f, this.alive));
            }
            this.headDist = this.GetCurveLerp(1, 0.5f, 1f);
            if (this.token != null)
            {
                this.keepDistance = Custom.LerpAndTick(this.keepDistance, Mathf.Sin(Mathf.Clamp01(this.token.glitch) * 3.1415927f) * this.alive, 0.006f, this.alive / ((this.keepDistance < this.token.glitch) ? 40f : 80f));
            }
            this.headDist = Mathf.Lerp(this.headDist, 50f, Mathf.Pow(this.keepDistance, 0.5f));
            Vector2 a = Custom.DirVec(Custom.InverseKinematic(this.basePos, this.armPos, this.armLength * 0.65f, this.armLength * 0.35f, this.flip), this.armPos);
            for (int i = 0; i < this.coord.GetLength(0); i++)
            {
                float num = Mathf.InverseLerp(-1f, (float)this.coord.GetLength(0), (float)i);
                Vector2 a2 = Custom.Bezier(this.armPos, this.armPos + a * this.coordLength * 0.5f, this.head, this.head - this.headDir * this.coordLength * 0.5f, num);
                this.coord[i, 1] = this.coord[i, 0];
                this.coord[i, 0] += this.coord[i, 2];
                this.coord[i, 2] *= 0.8f;
                this.coord[i, 2] += (a2 - this.coord[i, 0]) * Mathf.Lerp(0f, 0.25f, Mathf.Sin(num * 3.1415927f));
                this.coord[i, 0] += (a2 - this.coord[i, 0]) * Mathf.Lerp(0f, 0.25f, Mathf.Sin(num * 3.1415927f));
                if (i > 2)
                {
                    this.coord[i, 2] += Custom.DirVec(this.coord[i - 2, 0], this.coord[i, 0]);
                    this.coord[i - 2, 2] -= Custom.DirVec(this.coord[i - 2, 0], this.coord[i, 0]);
                }
                if (i > 3)
                {
                    this.coord[i, 2] += Custom.DirVec(this.coord[i - 3, 0], this.coord[i, 0]) * 0.5f;
                    this.coord[i - 3, 2] -= Custom.DirVec(this.coord[i - 3, 0], this.coord[i, 0]) * 0.5f;
                }
                if (num < 0.5f)
                {
                    this.coord[i, 2] += a * Mathf.InverseLerp(0.5f, 0f, num) * Mathf.InverseLerp(5f, 0f, (float)i);
                }
                else
                {
                    this.coord[i, 2] -= this.headDir * Mathf.InverseLerp(0.5f, 1f, num);
                }
            }
            this.ConnectCoord();
            this.ConnectCoord();
            for (int j = 0; j < this.coord.GetLength(0); j++)
            {
                SharedPhysics.TerrainCollisionData terrainCollisionData = this.scratchTerrainCollisionData.Set(this.coord[j, 0], this.coord[j, 1], this.coord[j, 2], 2f, new IntVector2(0, 0), true);
                terrainCollisionData = SharedPhysics.HorizontalCollision(this.room, terrainCollisionData);
                terrainCollisionData = SharedPhysics.VerticalCollision(this.room, terrainCollisionData);
                this.coord[j, 0] = terrainCollisionData.pos;
                this.coord[j, 2] = terrainCollisionData.vel;
            }
            for (int k = 0; k < this.curveLerps.GetLength(0); k++)
            {
                this.curveLerps[k, 1] = this.curveLerps[k, 0];
                this.curveLerps[k, 0] = Mathf.Min(1f, this.curveLerps[k, 0] + this.curveLerps[k, 4]);
            }
            base.Update(eu);
        }

        private void NewCurveLerp(int curveLerp, float to, float speed)
        {
            if (this.curveLerps[curveLerp, 0] < 1f || this.curveLerps[curveLerp, 1] < 1f)
            {
                return;
            }
            this.curveLerps[curveLerp, 2] = this.curveLerps[curveLerp, 3];
            this.curveLerps[curveLerp, 3] = to;
            this.curveLerps[curveLerp, 4] = speed / Mathf.Abs(this.curveLerps[curveLerp, 2] - this.curveLerps[curveLerp, 3]);
            this.curveLerps[curveLerp, 0] = 0f;
            this.curveLerps[curveLerp, 1] = 0f;
        }

        private float GetCurveLerp(int curveLerp, float sCurveK, float timeStacker)
        {
            return Mathf.Lerp(this.curveLerps[curveLerp, 2], this.curveLerps[curveLerp, 3], Custom.SCurve(Mathf.Lerp(this.curveLerps[curveLerp, 1], this.curveLerps[curveLerp, 0], timeStacker), sCurveK));
        }

        private void ConnectCoord()
        {
            this.coord[0, 2] -= Custom.DirVec(this.armPos, this.coord[0, 0]) * (Vector2.Distance(this.armPos, this.coord[0, 0]) - this.coordSeg);
            this.coord[0, 0] -= Custom.DirVec(this.armPos, this.coord[0, 0]) * (Vector2.Distance(this.armPos, this.coord[0, 0]) - this.coordSeg);
            for (int i = 1; i < this.coord.GetLength(0); i++)
            {
                if (!Custom.DistLess(this.coord[i - 1, 0], this.coord[i, 0], this.coordSeg))
                {
                    Vector2 a = Custom.DirVec(this.coord[i, 0], this.coord[i - 1, 0]) * (Vector2.Distance(this.coord[i - 1, 0], this.coord[i, 0]) - this.coordSeg);
                    this.coord[i, 2] += a * 0.5f;
                    this.coord[i, 0] += a * 0.5f;
                    this.coord[i - 1, 2] -= a * 0.5f;
                    this.coord[i - 1, 0] -= a * 0.5f;
                }
            }
            this.coord[this.coord.GetLength(0) - 1, 2] -= Custom.DirVec(this.head, this.coord[this.coord.GetLength(0) - 1, 0]) * (Vector2.Distance(this.head, this.coord[this.coord.GetLength(0) - 1, 0]) - this.coordSeg);
            this.coord[this.coord.GetLength(0) - 1, 0] -= Custom.DirVec(this.head, this.coord[this.coord.GetLength(0) - 1, 0]) * (Vector2.Distance(this.head, this.coord[this.coord.GetLength(0) - 1, 0]) - this.coordSeg);
        }

        public Vector2 EyePos(float timeStacker)
        {
            return Vector2.Lerp(this.lastHead, this.head, timeStacker) + (Vector2)Vector3.Slerp(this.lastHeadDir, this.headDir, timeStacker) * 3f;
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[this.TotalSprites];
            sLeaser.sprites[this.BaseSprite] = new FSprite("Circle20", true);
            sLeaser.sprites[this.BaseSprite].scaleX = 0.5f;
            sLeaser.sprites[this.BaseSprite].scaleY = 0.7f;
            sLeaser.sprites[this.BaseSprite].rotation = Custom.VecToDeg(this.mainDir);
            sLeaser.sprites[this.Arm1Sprite] = new FSprite("pixel", true);
            sLeaser.sprites[this.Arm1Sprite].scaleX = 4f;
            sLeaser.sprites[this.Arm1Sprite].anchorY = 0f;
            sLeaser.sprites[this.Arm2Sprite] = new FSprite("pixel", true);
            sLeaser.sprites[this.Arm2Sprite].scaleX = 3f;
            sLeaser.sprites[this.Arm2Sprite].anchorY = 0f;
            sLeaser.sprites[this.Arm3Sprite] = new FSprite("pixel", true);
            sLeaser.sprites[this.Arm3Sprite].scaleX = 1.5f;
            sLeaser.sprites[this.Arm3Sprite].scaleY = this.armLength * 0.6f;
            sLeaser.sprites[this.Arm3Sprite].anchorY = 0f;
            sLeaser.sprites[this.Arm4Sprite] = new FSprite("pixel", true);
            sLeaser.sprites[this.Arm4Sprite].scaleX = 3f;
            sLeaser.sprites[this.Arm4Sprite].scaleY = 8f;
            sLeaser.sprites[this.Arm5Sprite] = new FSprite("pixel", true);
            sLeaser.sprites[this.Arm5Sprite].scaleX = 6f;
            sLeaser.sprites[this.Arm5Sprite].scaleY = 8f;
            sLeaser.sprites[this.ArmJointSprite] = new FSprite("JetFishEyeA", true);
            sLeaser.sprites[this.LampSprite] = new FSprite("tinyStar", true);
            sLeaser.sprites[this.SocketSprite] = new FSprite("pixel", true);
            sLeaser.sprites[this.SocketSprite].scaleX = 5f;
            sLeaser.sprites[this.SocketSprite].scaleY = 9f;
            sLeaser.sprites[this.HeadSprite] = new FSprite("pixel", true);
            sLeaser.sprites[this.HeadSprite].scaleX = 4f;
            sLeaser.sprites[this.HeadSprite].scaleY = 6f;
            if (ModManager.MSC)
            {
                sLeaser.sprites[this.SataFlasher] = new FSprite("pixel", true);
                sLeaser.sprites[this.SataFlasher].isVisible = false;
            }
            for (int i = 0; i < this.coord.GetLength(0); i++)
            {
                sLeaser.sprites[this.CoordSprite(i)] = new FSprite("pixel", true);
                sLeaser.sprites[this.CoordSprite(i)].scaleX = ((i % 2 == 0) ? 2f : 3f);
                sLeaser.sprites[this.CoordSprite(i)].scaleY = 5f;
            }
            this.AddToContainer(sLeaser, rCam, null);
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            sLeaser.sprites[this.BaseSprite].x = this.basePos.x - camPos.x;
            sLeaser.sprites[this.BaseSprite].y = this.basePos.y - camPos.y;
            Vector2 vector = Vector2.Lerp(this.lastHead, this.head, timeStacker);
            Vector2 vector2 = Vector3.Slerp(this.lastHeadDir, this.headDir, timeStacker);
            Vector2 vector3 = Vector2.Lerp(this.lastArmPos, this.armPos, timeStacker);
            Vector2 vector4 = Custom.InverseKinematic(this.basePos, vector3, this.armLength * 0.65f, this.armLength * 0.35f, this.flip);
            sLeaser.sprites[this.Arm1Sprite].x = this.basePos.x - camPos.x;
            sLeaser.sprites[this.Arm1Sprite].y = this.basePos.y - camPos.y;
            sLeaser.sprites[this.Arm1Sprite].scaleY = Vector2.Distance(this.basePos, vector4);
            sLeaser.sprites[this.Arm1Sprite].rotation = Custom.AimFromOneVectorToAnother(this.basePos, vector4);
            sLeaser.sprites[this.Arm2Sprite].x = vector4.x - camPos.x;
            sLeaser.sprites[this.Arm2Sprite].y = vector4.y - camPos.y;
            sLeaser.sprites[this.Arm2Sprite].scaleY = Vector2.Distance(vector4, vector3);
            sLeaser.sprites[this.Arm2Sprite].rotation = Custom.AimFromOneVectorToAnother(vector4, vector3);
            sLeaser.sprites[this.SocketSprite].x = vector3.x - camPos.x;
            sLeaser.sprites[this.SocketSprite].y = vector3.y - camPos.y;
            sLeaser.sprites[this.SocketSprite].rotation = Custom.VecToDeg(Vector3.Slerp(Custom.DirVec(vector4, vector3), Custom.DirVec(vector3, Vector2.Lerp(this.coord[0, 1], this.coord[0, 0], timeStacker)), 0.4f));
            Vector2 vector5 = Vector2.Lerp(this.basePos, vector4, 0.3f);
            Vector2 vector6 = Vector2.Lerp(vector4, vector3, 0.4f);
            sLeaser.sprites[this.Arm3Sprite].x = vector5.x - camPos.x;
            sLeaser.sprites[this.Arm3Sprite].y = vector5.y - camPos.y;
            sLeaser.sprites[this.Arm3Sprite].rotation = Custom.AimFromOneVectorToAnother(vector5, vector6);
            sLeaser.sprites[this.Arm4Sprite].x = vector6.x - camPos.x;
            sLeaser.sprites[this.Arm4Sprite].y = vector6.y - camPos.y;
            sLeaser.sprites[this.Arm4Sprite].rotation = Custom.AimFromOneVectorToAnother(vector5, vector6);
            vector5 += Custom.DirVec(this.basePos, vector4) * (this.armLength * 0.1f + 2f);
            sLeaser.sprites[this.Arm5Sprite].x = vector5.x - camPos.x;
            sLeaser.sprites[this.Arm5Sprite].y = vector5.y - camPos.y;
            sLeaser.sprites[this.Arm5Sprite].rotation = Custom.AimFromOneVectorToAnother(this.basePos, vector4);
            sLeaser.sprites[this.LampSprite].x = vector5.x - camPos.x;
            sLeaser.sprites[this.LampSprite].y = vector5.y - camPos.y;
            sLeaser.sprites[this.LampSprite].color = Color.Lerp(this.lampOffCol, this.lampColor, Mathf.Lerp(this.lastLampPower, this.lampPower, timeStacker) * Mathf.Pow(UnityEngine.Random.value, 0.5f) * (0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(this.lastSinCounter, this.sinCounter, timeStacker) / 6f)));
            sLeaser.sprites[this.ArmJointSprite].x = vector4.x - camPos.x;
            sLeaser.sprites[this.ArmJointSprite].y = vector4.y - camPos.y;
            sLeaser.sprites[this.HeadSprite].x = vector.x - camPos.x;
            sLeaser.sprites[this.HeadSprite].y = vector.y - camPos.y;
            if (ModManager.MSC && this.forceSatellite && sLeaser.sprites[this.HeadSprite].element.name != "MiniSatellite")
            {
                sLeaser.sprites[this.HeadSprite].SetElementByName("MiniSatellite");
                sLeaser.sprites[this.HeadSprite].scaleX = 1f;
                sLeaser.sprites[this.HeadSprite].scaleY = 1f;
            }
            if (ModManager.MSC && this.forceSatellite)
            {
                sLeaser.sprites[this.HeadSprite].rotation = Custom.VecToDeg(vector2) - 90f;
                if (this.sataFlasherLight >= 99)
                {
                    sLeaser.sprites[this.SataFlasher].isVisible = !sLeaser.sprites[this.SataFlasher].isVisible;
                    this.sataFlasherLight = 0;
                }
                sLeaser.sprites[this.SataFlasher].color = Color.Lerp(Color.white, this.lampOffCol, UnityEngine.Random.value * 0.1f);
                sLeaser.sprites[this.SataFlasher].alpha = 0.9f + UnityEngine.Random.value * 0.09f;
                sLeaser.sprites[this.SataFlasher].x = vector.x + vector2.x * 5f - camPos.x;
                sLeaser.sprites[this.SataFlasher].y = vector.y + vector2.y * 5f - camPos.y;
            }
            else
            {
                sLeaser.sprites[this.HeadSprite].rotation = Custom.VecToDeg(vector2);
                if (ModManager.MSC)
                {
                    sLeaser.sprites[this.SataFlasher].isVisible = false;
                }
            }
            Vector2 p = vector3;
            for (int i = 0; i < this.coord.GetLength(0); i++)
            {
                Vector2 vector7 = Vector2.Lerp(this.coord[i, 1], this.coord[i, 0], timeStacker);
                sLeaser.sprites[this.CoordSprite(i)].x = vector7.x - camPos.x;
                sLeaser.sprites[this.CoordSprite(i)].y = vector7.y - camPos.y;
                sLeaser.sprites[this.CoordSprite(i)].rotation = Custom.AimFromOneVectorToAnother(p, vector7);
                p = vector7;
            }
            if (base.slatedForDeletetion || this.room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].color = palette.blackColor;
            }
            this.lampOffCol = Color.Lerp(palette.blackColor, new Color(1f, 1f, 1f), 0.15f);
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (newContatiner == null)
            {
                newContatiner = rCam.ReturnFContainer("Midground");
            }
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].RemoveFromContainer();
                if (ModManager.MSC && i == this.SataFlasher)
                {
                    rCam.ReturnFContainer("ForegroundLights").AddChild(sLeaser.sprites[this.SataFlasher]);
                }
                else
                {
                    newContatiner.AddChild(sLeaser.sprites[i]);
                }
            }
        }
    }
}
