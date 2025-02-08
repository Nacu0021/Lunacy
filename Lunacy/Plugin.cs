using BepInEx;
using System.Security.Permissions;
using System.Security;

#pragma warning disable CS0618
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace Lunacy
{
    using Insects;
    using CustomTokens;
    using PlacedObjects;

    [BepInPlugin("nacu.lunacy", "Lunacy", "1.63")]
    public class Plugin : BaseUnityPlugin
    {
        public static bool AppliedAlreadyDontDoItAgainPlease;
        public static bool AppliedAlreadyDontDoItAgainPleasePart2;
        internal static BepInEx.Logging.ManualLogSource logger;

        public void OnEnable()
        {
            logger = Logger;
            LunacyEnums.RegisterEnums();
            On.RainWorld.OnModsInit += OnModsInit;
            On.RainWorld.PostModsInit += PostModsInit;
            IL.Menu.InitializationScreen.Update += LunacyTokens.InitializationScreen_UpdateIL;
        }

        public void OnDisable()
        {
            logger = null;
            AppliedAlreadyDontDoItAgainPlease = false;
            AppliedAlreadyDontDoItAgainPleasePart2 = false;
        }

        private static void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld raingame)
        {
            orig.Invoke(raingame);

            if (!AppliedAlreadyDontDoItAgainPlease)
            {
                AppliedAlreadyDontDoItAgainPlease = true;

                Futile.atlasManager.LoadAtlas("Atlases/lunacy");
                ModDependantWorld.Apply();
                InsectHooks.Apply();
                Fireflies.Apply();
                PlacesObjects.Apply();
                CustomFairies.Apply();
                Metropolis.Apply();
                CustomSlimeMoldHooks.Apply();
                LunacyTokens.Apply();
            }
        }

        private static void PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            orig.Invoke(self);

            if (!AppliedAlreadyDontDoItAgainPleasePart2)
            {
                AppliedAlreadyDontDoItAgainPleasePart2 = true;

                LunacyTokens.AddCustomTokens();
            }
        }
    }
}