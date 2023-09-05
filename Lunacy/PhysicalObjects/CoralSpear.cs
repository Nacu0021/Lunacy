using UnityEngine;
using RWCustom;

namespace Lunacy.PhysicalObjects
{
    using PlacedObjects;

    public class CoralSpear : Spear
    {
        public CoralSpearCoral coral;
        public int snapCounter;
        public bool increment;

        public CoralSpearAbstract AbstractCoral { get; }
        public bool StuckToCoral => AbstractCoral.stuck;

        public CoralSpear(CoralSpearAbstract abstr) : base(abstr, abstr.world)
        {
            AbstractCoral = abstr;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            if (increment)
            {
                snapCounter++;
                if (snapCounter > 20) vibrate = 2;
                if (snapCounter > 40)
                {
                    coral.SnapOff();
                }
            }
            else snapCounter = 0;
            increment = false;
        }

        public override void SetRandomSpin()
        {
            if (AbstractCoral.stuck && snapCounter == 0) return;
            base.SetRandomSpin();
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            base.ApplyPalette(sLeaser, rCam, palette);
            color = Color.cyan;
        }
    }
}
