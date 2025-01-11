namespace Lunacy
{
    public class LunacyEnums
    {
        public static CosmeticInsect.Type LightningBugA;
        public static CosmeticInsect.Type LightningBugB;
        public static CosmeticInsect.Type Starfish;
        //public static CosmeticInsect.Type WaterStrider;

        public static RoomSettings.RoomEffect.Type LightningBugsA;
        public static RoomSettings.RoomEffect.Type LightningBugsB;
        public static RoomSettings.RoomEffect.Type Starfishies;
        //public static RoomSettings.RoomEffect.Type WaterStriders;

        public static PlacedObject.FairyParticleData.SpriteType Custom;

        public static PlacedObject.LightFixtureData.Type SlimeMoldLightA;
        public static PlacedObject.LightFixtureData.Type SlimeMoldLightB;

        public static SoundID ExportedMechActive;

        public static void RegisterEnums()
        {
            LightningBugA = new ("LightningBugA", true);
            LightningBugB = new ("LightningBugB", true);
            Starfish = new ("Starfish", true);
            //WaterStrider = new ("WaterStrider", true);

            LightningBugsA = new ("LightningBugsA", true);
            LightningBugsB = new ("LightningBugsB", true);
            Starfishies = new ("Starfishies", true);
            //WaterStriders = new ("WaterStriders", true);

            Custom = new("Custom", true);

            SlimeMoldLightA = new("SlimeMoldLightA", true);
            SlimeMoldLightB = new("SlimeMoldLightB", true);

            ExportedMechActive = new("lunacy_mech_active", true);
        }
    }
}
