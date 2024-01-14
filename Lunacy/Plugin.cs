using BepInEx;
using System.Security.Permissions;
using System.Security;
using Fisobs.Core;

#pragma warning disable CS0618
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace Lunacy
{
    using Insects;
    using Creatures;
    using Lunacy.PlacedObjects;
    using PhysicalObjects;

    [BepInPlugin("nacu.lunacy", "Lunacy", "1.2")]
    public class Plugin : BaseUnityPlugin
    {
        public static bool AppliedAlreadyDontDoItAgainPlease;

        internal static BepInEx.Logging.ManualLogSource logger;

        public void OnEnable()
        {
            logger = Logger;
            LunacyEnums.RegisterEnums();

            On.RainWorld.OnModsInit += OnModsInit;
        }

        public void OnDisable()
        {
            logger = null;
        }

        public static void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld raingame)
        {
            orig.Invoke(raingame);

            if (!AppliedAlreadyDontDoItAgainPlease)
            {
                AppliedAlreadyDontDoItAgainPlease = true;

                Futile.atlasManager.LoadAtlas("Atlases/lunacy");
                ModDependantWorld.Apply();
                InsectHooks.Apply();
                Fireflies.Apply();
            }
        }
    }
}