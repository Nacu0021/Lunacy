using MonoMod.Cil;
using System;
using System.Text.RegularExpressions;
using static ModManager.ModMerger;

namespace Lunacy
{
    public class ModDependantWorld
    {
        public static void Apply()
        {
            //On.ModManager.ModMerger.WorldDen.ctor += TestingTesting;
            On.ModManager.ModMerger.WorldRoomSpawn.SplitSpawners += WorldRoomSpawn_SplitSpawners;

            IL.WorldLoader.ctor_RainWorldGame_Name_bool_string_Region_SetupValues += il =>
            {
                //Plugin.logger.LogMessage($"Lunacy WorldLoader il start");

                ILCursor c = new(il);
                //Turns out this actually like doesnt isnt useful cause i decided to do it a different way but im keeping it here as a trophy
                //if (c.TryGotoNext(MoveType.Before,
                //    x => x.MatchStloc(8),
                //    x => x.MatchLdloc(5),
                //    x => x.MatchLdloc(7),
                //    x => x.MatchLdelemRef(),
                //    x => x.MatchLdcI4(0),
                //    x => x.MatchCallOrCallvirt<String>("get_Chars")
                //    ))
                //{
                //    c.Index += 5;
                //    c.Emit(OpCodes.Pop);
                //    c.Emit(OpCodes.Ldloc, 5);
                //    c.Emit(OpCodes.Ldloc, 7);
                //    c.Emit(OpCodes.Ldelem_Ref);
                //    c.EmitDelegate<Func<string, int>>((line) =>
                //    {
                //        if (line[0] == '[')
                //        {
                //            return line.IndexOf(']') + 1;
                //        }
                //        return 0;
                //    });
                //}
                //else Plugin.logger.LogError("NOOOOOOOOOOOO commando risk of rain 2 RUN COMING IN HOT " + il);

                //Do the thing
                if (c.TryGotoNext(MoveType.Before,
                    x => x.MatchLdloc(5),
                    x => x.MatchLdloc(7),
                    x => x.MatchLdloc(5),
                    x => x.MatchLdloc(7),
                    x => x.MatchLdelemRef(),
                    x => x.MatchLdloc(5),
                    x => x.MatchLdloc(7),
                    x => x.MatchLdelemRef(),
                    x => x.MatchLdstr(")")
                    ))
                {
                    c.Index += 5;
                    c.EmitDelegate<Func<string, string>>((line) =>
                    {
                        //Welcome to the creature spawn repair line
                        if (line.Contains("["))
                        {
                            //First we gotta separate the yolk from the white idk whats the white called actually
                            int intdex = line.LastIndexOf(':') + 2;
                            string header = line.Substring(0, intdex);
                            string spawns = PurgeThyEvil(line.Substring(intdex) + 2);

                            //Then Reassemble!
                            line = header + spawns;
                        }
                        return line;
                    });
                }
                else Plugin.logger.LogError("NOOOOOOOOOOOO ACRID RUN COMING IN HOT " + il);

                //Plugin.logger.LogMessage($"Lunacy WorldLoader il end");
            };
        }

        //public static void TestingTesting(On.ModManager.ModMerger.WorldDen.orig_ctor orig, WorldDen self, string denString)
        //{
        //    Plugin.logger.LogMessage($"{denString}");
        //    orig.Invoke(self, denString);
        //}

        //Gotta do it for the modify files separately cause of how the parsing works
        public static string[] WorldRoomSpawn_SplitSpawners(On.ModManager.ModMerger.WorldRoomSpawn.orig_SplitSpawners orig, WorldRoomSpawn self, string spawnString)
        {
            if (spawnString.Contains("[")) spawnString = PurgeThyEvil(spawnString);
            Plugin.logger.LogMessage(spawnString);
            return orig.Invoke(self, spawnString);
        }

        //Correct the creature spawns depending on which mods are active
        public static string PurgeThyEvil(string creaturesString)
        {
            string[] spawns = Regex.Split(creaturesString.Trim(), ", ");
            creaturesString = string.Empty;
            for (int i = 0; i < spawns.Length; i++)
            {
                string s = spawns[i];
                int square = s.IndexOf('[');
                if (square != -1) //If the square bracket exists
                {
                    //Get the id inside [square brackets]
                    int o = s.IndexOf("[") + 1;
                    int e = s.IndexOf("]", o);
                    string modid = s.Substring(o, e - o);
                    Plugin.logger.LogMessage($"MODIFICATIO ID: {modid}");

                    //Keep the spawn if mod present, alternate to another spawn if pipe symbol exists, if not then ignore the spawn
                    int pipe = s.IndexOf("|");
                    bool keep = false;
                    foreach (var mod in ModManager.ActiveMods)
                    {
                        if (mod.id == modid || 
                            ((modid == "MSC" || modid == "DP") && ModManager.MSC)) //Shorthand for the downpour dlc(/moreslugcats)
                        {
                            keep = true;
                        }
                    }
                    if (keep) spawns[i] = s.Substring(0, square);
                    else
                    {
                        //If theres a pipe symbol just do the entire loop again so that you can nest as much mod checks as you desire and its the simplest solution lol
                        bool lineage = !s.Substring(1, 2).Contains("-"); //Very jank way of checking if something is lineage but how else
                        bool smokingpipe = pipe != -1;
                        spawns[i] = (lineage ? string.Empty : s.Substring(0, s.IndexOf("-") + 1)) + (smokingpipe ? s.Substring(pipe + 1) : ("NONE" + (lineage ? "-1.0" : string.Empty)));
                        if (smokingpipe)
                        {
                            i--;
                            continue;
                        }
                    }
                }
                bool cant = spawns[i] == string.Empty;
                //Reassemble
                creaturesString = string.Concat(creaturesString, (i == 0 || cant) ? string.Empty : ", ", spawns[i]);
            }

            return creaturesString;
        }
    }
}
