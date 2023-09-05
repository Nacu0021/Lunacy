namespace Lunacy.PhysicalObjects
{
    public class CoralSpearAbstract : AbstractSpear
    {
        public bool stuck;

        public CoralSpearAbstract(World world, CoralSpear realized, WorldCoordinate pos, EntityID ID) : base(world, realized, pos, ID, false)
        {
            type = LunacyEnums.CoralSpear;
        }

        public override void Realize()
        {
            base.Realize();
            if (realizedObject == null) realizedObject = new CoralSpear(this);
        }
    }
}
