using BepInEx;
using UnityEngine;
using System.Security.Permissions;
using System.Security;
using DevInterface;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace Lunacy
{
    [BepInPlugin("nacu.lunacy", "Lunacy", "1.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static bool AppliedAlreadyDontDoItAgainPlease;

        public void OnEnable()
        {
            LunacyEnums.RegisterEnums();
            On.RainWorld.OnModsInit += OnModsInit;
        }

        public void OnDisable()
        {
            LunacyEnums.UnregisterEnums();
        }

        public static void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld raingame)
        {
            orig(raingame);

            if (!AppliedAlreadyDontDoItAgainPlease)
            {
                AppliedAlreadyDontDoItAgainPlease = true;

                On.InsectCoordinator.CreateInsect += CreateCustomInsect;
                On.InsectCoordinator.RoomEffectToInsectType += InsectCoordinator_RoomEffectToInsectType;
                On.DevInterface.RoomSettingsPage.DevEffectGetCategoryFromEffectType += RoomSettingsPage_DevEffectGetCategoryFromEffectType;
                On.Room.Loaded += Room_Loaded;
            }
        }

        public static void Room_Loaded(On.Room.orig_Loaded orig, Room room)
        {
            orig.Invoke(room);

            for (int i = 0; i < room.roomSettings.effects.Count; i++)
            {
                if (room.roomSettings.effects[i].type == LunacyEnums.LightningBugsA ||
                    room.roomSettings.effects[i].type == LunacyEnums.LightningBugsB)
                {
                    if (room.insectCoordinator == null)
                    {
                        room.insectCoordinator = new InsectCoordinator(room);
                        room.AddObject(room.insectCoordinator);
                    }
                    room.insectCoordinator.AddEffect(room.roomSettings.effects[i]);
                }
            }
        }

        public static void CreateCustomInsect(On.InsectCoordinator.orig_CreateInsect orig, InsectCoordinator self, CosmeticInsect.Type type, Vector2 pos, InsectCoordinator.Swarm swarm)
        {
            if (!InsectCoordinator.TileLegalForInsect(type, self.room, pos))
            {
                return;
            }
            if (self.room.world.rainCycle.TimeUntilRain < Random.Range(1200, 1600))
            {
                return;
            }
            CosmeticInsect insect = null;
            if (type == LunacyEnums.LightningBugA || type == LunacyEnums.LightningBugB)
            {
                insect = new LightningBug(self.room, pos, type == LunacyEnums.LightningBugA);
            }
            if (insect != null) {
                self.allInsects.Add(insect);
                if (swarm != null)
                {
                    swarm.members.Add(insect);
                    insect.mySwarm = swarm;
                }
                self.room.AddObject(insect);
                return;
            }
            orig.Invoke(self, type, pos, swarm);
        }

        public static CosmeticInsect.Type InsectCoordinator_RoomEffectToInsectType(On.InsectCoordinator.orig_RoomEffectToInsectType orig, RoomSettings.RoomEffect.Type type)
        {
            if (type == LunacyEnums.LightningBugsA)
            {
                return LunacyEnums.LightningBugA;
            }
            if (type == LunacyEnums.LightningBugsB)
            {
                return LunacyEnums.LightningBugB;
            }

            return orig.Invoke(type);
        }

        public static RoomSettingsPage.DevEffectsCategories RoomSettingsPage_DevEffectGetCategoryFromEffectType(On.DevInterface.RoomSettingsPage.orig_DevEffectGetCategoryFromEffectType orig, RoomSettingsPage self, RoomSettings.RoomEffect.Type type)
        {
            if (type == LunacyEnums.LightningBugsA || type == LunacyEnums.LightningBugsB)
            {
                return RoomSettingsPage.DevEffectsCategories.Insects;
            }
            return orig.Invoke(self, type);
        }
    }
}