using UnityEngine;
using RWCustom;
using System.Collections.Generic;

namespace Lunacy.Insects
{
    //public class WaterStrider : CosmeticInsect
    //{
    //    public float size;
    //    public float depth;
    //    public float lastDepth;
    //
    //    public WaterStrider(Room room, Vector2 pos) : base(room, pos, LunacyEnums.WaterStrider)
    //    {
    //        size = 10f;
    //        Plugin.logger.LogMessage("spawning water strider");
    //        if (!room.water) Destroy();
    //    }
    //
    //    public override void Update(bool eu)
    //    {
    //        base.Update(eu);
    //
    //        lastDepth = depth;
    //        depth += 0.005f;
    //        if (depth > 1f) { depth = 0f; lastDepth = 0f; };
    //
    //        Plugin.logger.LogMessage($"Chillin at {pos.y}");
    //    }
    //
    //    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    //    {
    //        sLeaser.sprites = new FSprite[1];
    //
    //        sLeaser.sprites[0] = new FSprite("Circle20", true)
    //        {
    //            scale = (1f / 20f) * size,
    //            shader = rCam.game.rainWorld.Shaders["SpecificDepth"]
    //        };
    //        base.InitiateSprites(sLeaser, rCam);
    //        AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("GrabShaders"));
    //    }
    //
    //    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    //    {
    //        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    //
    //        Vector2 positio = Vector2.Lerp(lastPos, pos, timeStacker);
    //        Vector2 depthPoint = new Vector2(rCam.sSize.x / 2f, rCam.sSize.y * 0.6666667f);
    //        Water.SurfacePoint point0 = room.waterObject.surface[room.waterObject.ClosestSurfacePoint(pos.x), 0];
    //        Water.SurfacePoint point1 = room.waterObject.surface[room.waterObject.ClosestSurfacePoint(pos.x), 1];
    //        Vector2 point0Pos = Custom.ApplyDepthOnVector(point0.defaultPos + Vector2.Lerp(point0.lastPos, point0.pos, timeStacker) - camPos + new Vector2(0f, room.waterObject.cosmeticSurfaceDisplace), depthPoint, -10f);
    //        Vector2 point1Pos = Custom.ApplyDepthOnVector(point1.defaultPos + Vector2.Lerp(point1.lastPos, point1.pos, timeStacker) - camPos + new Vector2(0f, room.waterObject.cosmeticSurfaceDisplace), depthPoint, 30f);
    //        //Mathf.Abs(Mathf.Sin(Mathf.Lerp(lastDepth, depth, timeStacker) * Mathf.PI * 2f))
    //        positio.y = Mathf.Lerp(point0Pos.y, point1Pos.y, 1f);
    //        positio.x -= camPos.x;
    //
    //        sLeaser.sprites[0].SetPosition(positio);
    //        sLeaser.sprites[0].alpha = 1f - 0.36666667f;//- Mathf.Abs(Mathf.Sin(Mathf.Lerp(lastDepth, depth, timeStacker) * Mathf.PI * 2f));
    //    }
    //
    //    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    //    {
    //        sLeaser.sprites[0].color = Color.red;
    //    }
    //}
}
