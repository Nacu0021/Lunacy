namespace Lunacy
{
    public class LunacyEnums
    {
        public static CosmeticInsect.Type LightningBugA;
        public static CosmeticInsect.Type LightningBugB;
        public static RoomSettings.RoomEffect.Type LightningBugsA;
        public static RoomSettings.RoomEffect.Type LightningBugsB;

        public static void RegisterEnums()
        {
            LightningBugA = new CosmeticInsect.Type("LightningBugA", true);
            LightningBugB = new CosmeticInsect.Type("LightningBugB", true);
            LightningBugsA = new RoomSettings.RoomEffect.Type("LightningBugsA", true);
            LightningBugsB = new RoomSettings.RoomEffect.Type("LightningBugsB", true);
        }

        public static void UnregisterEnums()
        {
            if (LightningBugA != null) { LightningBugA.Unregister(); LightningBugA = null; }
            if (LightningBugB != null) { LightningBugB.Unregister(); LightningBugB = null; }
            if (LightningBugsA != null) { LightningBugsA.Unregister(); LightningBugsA = null; }
            if (LightningBugsB != null) { LightningBugsB.Unregister(); LightningBugsB = null; }
        }
    }
}
