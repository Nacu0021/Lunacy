using UnityEngine;
using RWCustom;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System.Reflection;
using System.Collections.Generic;
using System;
using MoreSlugcats;

namespace Lunacy
{
    using PhysicalObjects;

    //public class PlayerHooks
    //{
    //    public static void Apply()
    //    {
    //        IL.Player.GraphicsModuleUpdated += Player_GraphicsModuleUpdatedIL;
    //        IL.Player.PickupCandidate += Player_PickupCandidate;
    //        IL.Player.GrabUpdate += Player_GrabUpdateIL;
    //    }
    //
    //    // Ignore throwing main hand if coral spear on stem
    //    public static void Player_GrabUpdateIL(ILContext il)
    //    {
    //        ILCursor c = new(il);
    //        if (c.TryGotoNext(
    //            x => x.MatchLdarg(0),
    //            x => x.MatchLdcI4(5),
    //            x => x.MatchStfld<Player>("wantToThrow"),
    //            x => x.MatchLdarg(0),
    //            x => x.MatchLdfld<Player>("wantToThrow"),
    //            x => x.MatchLdcI4(0)
    //            ))
    //        {
    //            c.Index += 2;
    //            c.Emit(OpCodes.Ldarg_0);
    //            c.Emit(OpCodes.Ldarg_1);
    //            c.EmitDelegate<Func<int, Player, bool, int>>((original, player, eu) =>
    //            {
    //                for (int i = 0; i < 2; i++)
    //                {
    //                    if (player.grasps[i] != null && player.grasps[i].grabbed is CoralSpear s && s.StuckToCoral)
    //                    {
    //                        int noti = i == 0 ? 1 : 0;
    //                        if (player.grasps[noti] != null && player.IsObjectThrowable(player.grasps[noti].grabbed))
    //                        {
    //                            player.ThrowObject(i, eu);
    //                        }
    //                        original = 0;
    //                        break;
    //                    }
    //                }
    //
    //                return original;
    //            });
    //        }
    //        else Plugin.logger.LogError("NOOOOOOOOOOOO ACRID RUN COMING IN HOT " + il);
    //    }
    //
    //    // Change the range required for a pickup candidate for coral spears
    //    public static void Player_PickupCandidate(ILContext il)
    //    {
    //        ILCursor c = new(il);
    //        if (c.TryGotoNext(MoveType.Before,
    //            x => x.MatchLdcR4(40),
    //            x => x.MatchAdd()))
    //        {
    //            c.Index++;
    //            c.Emit(OpCodes.Ldarg_0);
    //            c.Emit(OpCodes.Ldloc_2);
    //            c.Emit(OpCodes.Ldloc_3);
    //            c.EmitDelegate<Func<float, Player, int, int, float>>((orig, player, i, j) =>
    //            {
    //                if (player.room.physicalObjects[i][j] is CoralSpear s && s.StuckToCoral)
    //                {
    //                    orig = 20f;
    //                }
    //
    //                return orig;
    //            });
    //        } 
    //        else Plugin.logger.LogMessage("Pickup candidate erore " + il);
    //    }
    //
    //    // Coral spear behavior
    //    public static void Player_GraphicsModuleUpdatedIL(ILContext il)
    //    {
    //        ILCursor c = new(il); 
    //        ILCursor l = new(il); // Label cursor
    //
    //        if (l.TryGotoNext(MoveType.After,
    //            x => x.MatchLdcR4(0),
    //            x => x.MatchStfld<Weapon>("rotationSpeed")))
    //        {
    //            ILLabel label = l.DefineLabel();
    //            l.MarkLabel(label);
    //
    //            if (c.TryGotoNext(MoveType.Before,
    //               x => x.MatchLdarg(0),
    //               x => x.MatchCallOrCallvirt<Creature>("get_grasps"),
    //               x => x.MatchLdloc(0),
    //               x => x.MatchLdelemRef(),
    //               x => x.MatchLdfld<Creature.Grasp>("grabbed"),
    //               x => x.MatchCallOrCallvirt<PhysicalObject>("get_firstChunk"),
    //               x => x.MatchLdarg(0),
    //               x => x.MatchCallOrCallvirt<PhysicalObject>("get_graphicsModule"),
    //               x => x.MatchIsinst<PlayerGraphics>(),
    //               x => x.MatchLdfld<PlayerGraphics>("hands")))
    //            {
    //                c.Emit(OpCodes.Ldarg_0);
    //                c.Emit(OpCodes.Ldloc, 0);
    //                c.Emit(OpCodes.Ldarg_2);
    //                c.Emit(OpCodes.Ldc_I4_0);
    //                c.EmitDelegate<Func<Player, int, bool, int, int>>((player, i, eu, orig) =>
    //                {
    //                    // Actual code time wahoo
    //                    if (player.grasps[i].grabbed is CoralSpear s && s.StuckToCoral)
    //                    {
    //                        if (player.input[0].jmp)
    //                        {
    //                            s.forbiddenToPlayer = 10;
    //                            player.ReleaseGrasp(i);
    //                            Vector2 veloucity = Vector2.up * 9f * Mathf.Abs(Vector2.Dot(Vector2.right, s.rotation));
    //                            player.bodyChunks[0].vel += veloucity;
    //                            player.bodyChunks[1].vel += veloucity;
    //                        }
    //                        if (player.enteringShortCut != null) player.ReleaseGrasp(i);
    //                        
    //                        Vector2 vetore = Vector2.zero;
    //                        if (player.input[0].pckp)
    //                        {
    //                            if (player.spearOnBack != null) player.spearOnBack.increment = false;
    //                            if (player.slugOnBack != null) player.slugOnBack.increment = false;
    //                            player.swallowAndRegurgitateCounter = 0;
    //                            if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear) (player.graphicsModule as PlayerGraphics).tailSpecks.setSpearProgress(0);
    //                        
    //                            float fac = Mathf.InverseLerp(0, 40, s.snapCounter);
    //                            s.increment = true;
    //                            s.setRotation = new Vector2?(Vector3.Slerp(s.rot, s.rot + Custom.DirVec(s.firstChunk.pos, player.mainBodyChunk.pos) * 0.1f, fac).normalized);
    //                            vetore = Custom.DirVec(s.firstChunk.pos, player.mainBodyChunk.pos) * 4f * fac;
    //                        
    //                            (player.graphicsModule as PlayerGraphics).lookDirection = Custom.DirVec((player.graphicsModule as PlayerGraphics).drawPositions[0, 0], s.firstChunk.pos);
    //                            if (s.snapCounter > s.maxSnapCounter / 4.5f)
    //                            {
    //                                (player.graphicsModule as PlayerGraphics).blink = 2;
    //                                (player.graphicsModule as PlayerGraphics).head.vel += Custom.RNV();
    //                            }
    //                        }
    //                        
    //                        SlugcatHand hand = (player.graphicsModule as PlayerGraphics).hands[i];
    //                        hand.mode = Limb.Mode.HuntAbsolutePosition;
    //                        hand.absoluteHuntPos = s.firstChunk.pos + Custom.PerpendicularVector((hand.connection.pos - s.firstChunk.pos).normalized) * 2f * ((hand.limbNumber == 0) ? -1f : 1f);
    //                        hand.huntSpeed = 30f;
    //                        hand.quickness = 1f;
    //                        
    //                        Vector2 dir = Custom.DirVec(hand.connection.pos, s.firstChunk.pos);
    //                        float dist = Vector2.Distance(hand.connection.pos, s.firstChunk.pos);
    //                        float rad = 20f - vetore.magnitude;
    //                        //
    //                        if (dist > rad)
    //                        {
    //                            if (dist > 100f) player.ReleaseGrasp(i);
    //                            else
    //                            {
    //                                player.mainBodyChunk.pos += dir * (dist - rad);
    //                                player.mainBodyChunk.vel += dir * (dist - rad);
    //                            }
    //                        }
    //                        s.rotationSpeed = 0;
    //                        
    //                        orig = 1;
    //                    }
    //
    //                    return orig;
    //                });
    //                c.Emit(OpCodes.Brtrue, label);
    //            }
    //            else Plugin.logger.LogMessage("Hand problems i think " + il);
    //        }
    //        else Plugin.logger.LogMessage("LAbel roratiotone speede " + il);
    //    }
    //}
}
