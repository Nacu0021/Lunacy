using UnityEngine;
using DevInterface;

namespace Lunacy.Insects
{
    public class InsectHooks
    {
        public static void Apply()
        {
            On.InsectCoordinator.SpeciesDensity_Type_1 += InsectCoordinator_SpeciesDensity_Type_1;
            On.InsectCoordinator.TileLegalForInsect += InsectCoordinator_TileLegalForInsect;
            On.InsectCoordinator.CreateInsect += CreateCustomInsect;
            On.InsectCoordinator.RoomEffectToInsectType += InsectCoordinator_RoomEffectToInsectType;
            On.DevInterface.RoomSettingsPage.DevEffectGetCategoryFromEffectType += RoomSettingsPage_DevEffectGetCategoryFromEffectType;
            On.Room.Loaded += Room_Loaded;
        }

        public static float InsectCoordinator_SpeciesDensity_Type_1(On.InsectCoordinator.orig_SpeciesDensity_Type_1 orig, CosmeticInsect.Type type)
        {
            //if (type == LunacyEnums.LightningBugA || type == LunacyEnums.LightningBugB)
            //{
            //    return 0.9f;
            //}
            //
            //if (type == LunacyEnums.Starfish)
            //{
            //    return 0.8f;
            //}
            //if (type == LunacyEnums.WaterStrider)
            //{
            //    return 3f;
            //}

            return orig.Invoke(type);
        }

        public static bool InsectCoordinator_TileLegalForInsect(On.InsectCoordinator.orig_TileLegalForInsect orig, CosmeticInsect.Type type, Room room, Vector2 testPos)
        {
            if (type == LunacyEnums.LightningBugA || type == LunacyEnums.LightningBugB)
            {
                return !room.GetTile(testPos).AnyWater;
            }

            if (type == LunacyEnums.Starfish)
            {
                return room.GetTile(testPos).DeepWater;
            }

            //if (type == LunacyEnums.WaterStrider)
            //{
            //    return room.GetTile(testPos).WaterSurface;
            //}

            return orig.Invoke(type, room, testPos);
        }

        public static void Room_Loaded(On.Room.orig_Loaded orig, Room room)
        {
            orig.Invoke(room);

            for (int i = 0; i < room.roomSettings.effects.Count; i++)
            {
                if (room.roomSettings.effects[i].type == LunacyEnums.LightningBugsA ||
                    room.roomSettings.effects[i].type == LunacyEnums.LightningBugsB ||
                    room.roomSettings.effects[i].type == LunacyEnums.Starfishies)// ||
                    //room.roomSettings.effects[i].type == LunacyEnums.WaterStriders)
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
            else if (type == LunacyEnums.Starfish)
            {
                insect = new Starfish(self.room, pos);
            }
            //else if (type == LunacyEnums.WaterStrider)
            //{
            //    insect = new WaterStrider(self.room, pos);
            //}
            if (insect != null)
            {
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
            if (type == LunacyEnums.Starfishies)
            {
                return LunacyEnums.Starfish;
            }
            //if (type == LunacyEnums.WaterStriders)
            //{
            //    return LunacyEnums.WaterStrider;
            //}

            return orig.Invoke(type);
        }

        public static RoomSettingsPage.DevEffectsCategories RoomSettingsPage_DevEffectGetCategoryFromEffectType(On.DevInterface.RoomSettingsPage.orig_DevEffectGetCategoryFromEffectType orig, RoomSettingsPage self, RoomSettings.RoomEffect.Type type)
        {
            if (type == LunacyEnums.LightningBugsA || type == LunacyEnums.LightningBugsB || type == LunacyEnums.Starfishies)// || type == LunacyEnums.WaterStriders)
            {
                return RoomSettingsPage.DevEffectsCategories.Insects;
            }
            return orig.Invoke(self, type);
        }
    }
}
