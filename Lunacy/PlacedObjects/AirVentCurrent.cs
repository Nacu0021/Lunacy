using UnityEngine;
using RWCustom;
using static Pom.Pom;

namespace Lunacy.PlacedObjects
{
    public class AirVentCurrent : UpdatableAndDeletable
    {
        public PlacedObject obj;
        public bool circle;
        public FloatRect rect;
        public float radius;
        public Vector2 angle;
        public float strength;
        public float airRefill;
        public bool bubbles;
        public bool sound;
        public bool pushPhysical;
        public int spawnBubblesCounter;
        public PositionedSoundEmitter soundEmitter;
        public float soundVol;
        public bool initialized;

        public ManagedData data => obj.data as ManagedData;

        public AirVentCurrent(Room room, PlacedObject obj) : base()
        {
            this.room = room;
            this.obj = obj;
            circle = false;
        }

        public void Init()
        {
            bubbles = data.GetValue<bool>("bubbles");
            sound = data.GetValue<bool>("sound");
            airRefill = data.GetValue<float>("air");
            if (circle)
            {
                radius = data.GetValue<Vector2>("handle").magnitude;
            }
            else
            {
                Vector2 handlePos = obj.pos + data.GetValue<Vector2>("handle");
                float left = handlePos.x < obj.pos.x ? handlePos.x : obj.pos.x;
                float bottom = handlePos.y < obj.pos.y ? handlePos.y : obj.pos.y;
                rect = new FloatRect(left, bottom, left == handlePos.x ? obj.pos.x : handlePos.x, bottom == handlePos.y ? obj.pos.y : handlePos.y);
                strength = data.GetValue<float>("strength");
                angle = Custom.DegToVec(data.GetValue<float>("angle"));
                pushPhysical = data.GetValue<bool>("push");
            }
            initialized = true;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (!initialized)
            {
                Init();
            }

            bool decreaseVol = true;
            foreach (var uad in room.updateList)
            {
                if (uad is PhysicalObject p)
                {
                    bool inRange = false;
                    foreach (BodyChunk chunk in p.bodyChunks)
                    {
                        bool r = false;
                        if (circle)
                        {
                            float dist = Vector2.Distance(chunk.pos, obj.pos);
                            if (dist < radius + chunk.rad)
                            {
                                inRange = true;
                                //float fac = Mathf.InverseLerp(chunk.rad, 0f, dist - radius);
                                //chunk.pos += angle * strength * fac * 0.5f;
                                //chunk.vel += angle * strength * fac * 0.5f;
                            }
                        }
                        else
                        {
                            float dist = Custom.VectorRectDistance(chunk.pos, rect);
                            if (dist < chunk.rad)
                            {
                                inRange = true;
                                if (pushPhysical)
                                {
                                    float fac = Mathf.InverseLerp(chunk.rad, 0f, dist);
                                    chunk.pos += angle * strength * fac * 0.5f;
                                    chunk.vel += angle * strength * fac * 0.5f;
                                }
                            }
                        }
                    }

                    if (airRefill > 0f && p is AirBreatherCreature crit)
                    {
                        crit.lungs = Mathf.Min(1f, crit.lungs + 0.004761905f * airRefill);
                    }

                    if (p is Player playe && inRange)
                    {
                        playe.airInLungs = Mathf.Min(1f, playe.airInLungs + 0.004761905f * airRefill);

                        if (!ModManager.MSC || !playe.isNPC)
                        {
                            soundVol = Mathf.Min(1f, soundVol + 0.05f);
                            decreaseVol = false;
                        }
                    }
                }
                else if (uad is CosmeticSprite c)
                {
                    float dist = Custom.VectorRectDistance(c.pos, rect);
                    if (dist == 0f)
                    {
                        c.vel += angle * strength;

                        if (c is Bubble)
                        {
                            float num2 = 0f;
                            if (room.waterObject != null)
                            {
                                num2 = room.waterObject.viscosity;
                            }
                            Vector2 b = Custom.DegToVec(-90f + 180f * UnityEngine.Random.value) * UnityEngine.Random.value * 1.2f * (1f - num2);
                            if (room.waterInverted)
                            {
                                c.vel += b;
                            }
                            else
                            {
                                c.vel -= b;
                            }
                        }
                    }
                }
            }

            if (decreaseVol)
            {
                soundVol = Mathf.Max(0f, soundVol - 0.05f);
            }

            if (room.BeingViewed)
            {
                if (bubbles)
                {
                    spawnBubblesCounter--;
                    if (spawnBubblesCounter <= 0)
                    {
                        Vector2 randomPos = default;
                        if (circle)
                        {
                            randomPos = obj.pos + Custom.RNV() * radius * Random.value;
                        }
                        else
                        {
                            randomPos = Custom.RandomPointInRect(rect);
                        }
                        if (room.PointSubmerged(randomPos))
                        {
                            room.AddObject(new Bubble(randomPos, Vector2.zero, false, false));
                            spawnBubblesCounter = (int)(UnityEngine.Random.Range(1, 3) / (airRefill));
                        }
                    }
                }
                if (sound)
                {
                    if (soundEmitter == null)
                    {
                        if (circle)
                        {
                            soundEmitter = new PositionedSoundEmitter(obj.pos, soundVol, data.GetValue<float>("pitch"));
                        }
                        else
                        {
                            soundEmitter = new RectSoundEmitter(rect, soundVol, data.GetValue<float>("pitch"));
                        }
                        room.PlaySound(LunacyEnums.ExportedMechActive, soundEmitter, true, soundVol, soundEmitter.pitch, false);
                        soundEmitter.requireActiveUpkeep = true;
                    }
                    soundEmitter.alive = true;
                    soundEmitter.volume = soundVol;
                    if (soundEmitter.slatedForDeletetion && !soundEmitter.soundStillPlaying)
                    {
                        soundEmitter = null;
                    }
                }
            }
        }
    }

    public class CircleVent : AirVentCurrent
    {
        public CircleVent(Room room, PlacedObject obj) : base(room, obj)
        {
            circle = true;
        }
    }
}
