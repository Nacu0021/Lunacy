using Fisobs.Properties;
using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Sandbox;
using UnityEngine;
using MoreSlugcats;
using RWCustom;
using Expedition;

namespace Lunacy.PhysicalObjects
{
    //public class CoralSpearFisob : Fisob
    //{
    //    public CoralSpearFisob() : base(LunacyEnums.CoralSpear)
    //    {
    //        Icon = new SimpleIcon("Symbol_Spear", Color.cyan); // Rember to change this someday
    //        SandboxPerformanceCost = new(linear: 0.2f, exponential: 0f);
    //        RegisterUnlock(LunacyEnums.CoralSpearUnlock, MultiplayerUnlocks.SandboxUnlockID.Slugcat);
    //    }
    //
    //    public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData, SandboxUnlock? unlock)
    //    {
    //        return new CoralSpearAbstract(world, null, saveData.Pos, saveData.ID, 40);
    //    }
    //
    //    public override ItemProperties Properties(PhysicalObject forObject)
    //    {
    //        if (forObject is CoralSpear spear)
    //        {
    //            return new CoralSpearProperties(spear);
    //        }
    //
    //        return null;
    //    }
    //}
    //
    //public class CoralSpearProperties : ItemProperties // Tweak these values to be exactly the same as spear's later
    //{
    //    public CoralSpear spear;
    //
    //    public CoralSpearProperties(CoralSpear spear)
    //    {
    //        this.spear = spear;
    //    }
    //
    //    public override void Throwable(Player player, ref bool throwable) => throwable = true;
    //    public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
    //    {
    //        bool dualWield = player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear || (ModManager.Expedition && Custom.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains("unl-dualwield"));
    //        grabability = dualWield ? Player.ObjectGrabability.OneHand : Player.ObjectGrabability.BigOneHand;
    //    }
    //    public override void ScavCollectScore(Scavenger scav, ref int score)
    //    {
    //        score = spear.StuckToCoral ? -1 : 3;
    //    }
    //    public override void LethalWeapon(Scavenger scav, ref bool isLethal) => isLethal = true;
    //    public override void ScavWeaponPickupScore(Scavenger scav, ref int score)
    //    {
    //        score = spear.StuckToCoral ? -1 : 3;
    //    }
    //    public override void ScavWeaponUseScore(Scavenger scav, ref int score)
    //    {
    //        score = spear.StuckToCoral ? -1 : 3;
    //    }
    //}
}
