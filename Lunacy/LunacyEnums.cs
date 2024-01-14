namespace Lunacy
{
    public class LunacyEnums
    {
        public static CosmeticInsect.Type LightningBugA;
        public static CosmeticInsect.Type LightningBugB;
        public static CosmeticInsect.Type Starfish;

        public static RoomSettings.RoomEffect.Type LightningBugsA;
        public static RoomSettings.RoomEffect.Type LightningBugsB;
        public static RoomSettings.RoomEffect.Type Starfishies;

        public static void RegisterEnums()
        {
            LightningBugA = new CosmeticInsect.Type("LightningBugA", true);
            LightningBugB = new CosmeticInsect.Type("LightningBugB", true);
            Starfish = new CosmeticInsect.Type("Starfish", true);

            LightningBugsA = new RoomSettings.RoomEffect.Type("LightningBugsA", true);
            LightningBugsB = new RoomSettings.RoomEffect.Type("LightningBugsB", true);
            Starfishies = new RoomSettings.RoomEffect.Type("Starfishies", true);
        }
    }
}
