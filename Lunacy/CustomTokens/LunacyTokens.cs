using System;
using System.Collections.Generic;
using DevInterface;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using Menu;
using System.IO;
using RWCustom;
using UnityEngine;

namespace Lunacy.CustomTokens
{
    public static class LunacyTokens
    {
        public static Dictionary<string, CustomTokenDefinition> CustomTokenDefinitions = [];
        public static ConditionalWeakTable<PlayerProgression.MiscProgressionData, List<CustomTokenProgression>> CustomTokenProgressions = new();
        internal static Dictionary<string, Dictionary<string, List<string>>> regionCustomTokens = new();
        internal static Dictionary<string, Dictionary<string, List<List<SlugcatStats.Name>>>> regionCustomTokensAccessibility = new();

        public class CustomTokenProgression
        {
            public string tokenID;
            public List<string> unlockedTokens;

            public CustomTokenProgression(string tokenID)
            {
                this.tokenID = tokenID;
                unlockedTokens = [];
            }

            public override string ToString()
            {
                return string.Join("<mpdB>", unlockedTokens);
            }
        }

        internal static void Apply()
        {
            On.DevInterface.ObjectsPage.CreateObjRep += ObjectsPage_CreateObjRep;
            On.PlacedObject.GenerateEmptyData += PlacedObject_GenerateEmptyData;
            On.DevInterface.ObjectsPage.DevObjectGetCategoryFromPlacedType += ObjectsPage_DevObjectGetCategoryFromPlacedType;
            On.PlayerProgression.MiscProgressionData.ctor += MiscProgressionData_ctor;
            On.PlayerProgression.MiscProgressionData.ToString += MiscProgressionData_ToString;
            On.PlayerProgression.MiscProgressionData.FromString += MiscProgressionData_FromString;
            On.RainWorld.ClearTokenCacheInMemory += RainWorld_ClearTokenCacheInMemory;
            IL.RainWorld.BuildTokenCache += RainWorld_BuildTokenCacheIL;
            On.RainWorld.BuildTokenCache += RainWorld_BuildTokenCache; ;
            IL.RainWorld.ReadTokenCache += RainWorld_ReadTokenCacheIL;
            //IL.MoreSlugcats.CollectiblesTracker.ctor += CollectiblesTracker_ctorIL;
            On.MoreSlugcats.CollectiblesTracker.ctor += CollectiblesTracker_ctor;
        }

        internal static void AddCustomTokens()
        {
            foreach (KeyValuePair<string, CustomTokenDefinition> kvp in CustomTokenDefinitions)
            {
                new PlacedObject.Type(kvp.Key, true);
                if (!ObjectsPage.DevObjectCategories.values.entries.Contains(kvp.Value.devtoolsCategory))
                {
                    new ObjectsPage.DevObjectCategories(kvp.Value.devtoolsCategory, true);
                }
                regionCustomTokens[kvp.Key] = new();
                regionCustomTokensAccessibility[kvp.Key] = new();
            }
        }

        public static void InitializationScreen_UpdateIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdloc(16),
                x => x.MatchBrtrue(out _),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<InitializationScreen>("filesInBadState"),
                x => x.MatchBrfalse(out _)
                ))
            {
                c.MoveAfterLabels();
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc, 16);
                c.EmitDelegate<Func<InitializationScreen, bool, bool>>((self, flag2) =>
                {
                    if (!flag2)
                    {
                        bool forceRecomputeTokenCache = false;
                        string text4 = string.Concat(new string[]
                            {
                                Custom.RootFolderDirectory(),
                                Path.DirectorySeparatorChar.ToString(),
                                "mergedmods",
                                Path.DirectorySeparatorChar.ToString(),
                                "World",
                                Path.DirectorySeparatorChar.ToString(),
                                "IndexMaps"
                            }).ToLowerInvariant();
                        Directory.CreateDirectory(text4);

                        if (File.Exists(text4 + Path.DirectorySeparatorChar + "recomputetokencache.txt"))
                        {
                            return true;
                        }

                        List<string> forceRecomputeRegions = [];
                        string[] allRegions = [];
                        string regionsPath = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "regions.txt");
                        if (!File.Exists(regionsPath))
                        {
                            return flag2;
                        }
                        allRegions = File.ReadAllLines(regionsPath);
                        foreach (var region in allRegions)
                        {
                            string forceRecomputePath = AssetManager.ResolveFilePath("world" + Path.DirectorySeparatorChar + region + Path.DirectorySeparatorChar + "forcetokencache.txt");
                           
                            if (!File.Exists(forceRecomputePath)) continue;
                            string contents = File.ReadAllText(forceRecomputePath);
                            if (!string.IsNullOrEmpty(contents))
                            {
                                if (contents.ToLowerInvariant() == "always")
                                {
                                    Plugin.logger.LogMessage("Forcing token cache recompute from " + region);
                                    forceRecomputeTokenCache = true;
                                    break;
                                }
                                else if (contents.ToLowerInvariant() == "lunacy")
                                {
                                    string lunacyTokenCacheRegionPath = AssetManager.ResolveFilePath("world" + Path.DirectorySeparatorChar + "indexmaps" + Path.DirectorySeparatorChar + "lunacytokencache" + region.ToLowerInvariant() + ".txt");
                                    if (File.Exists(lunacyTokenCacheRegionPath)) continue;

                                    Plugin.logger.LogMessage("Forcing token cache recompute from " + region);
                                    forceRecomputeTokenCache = true;
                                    break;
                                }
                            }

                            if (forceRecomputeTokenCache) break;

                            string tokenCacheRegionPath = AssetManager.ResolveFilePath("world" + Path.DirectorySeparatorChar + "indexmaps" + Path.DirectorySeparatorChar + "tokencache" + region.ToLowerInvariant() + ".txt");
                            if (File.Exists(tokenCacheRegionPath))
                            {
                                string cacheContents = File.ReadAllText(tokenCacheRegionPath);
                                bool empty = true;
                                foreach (char ch in cacheContents)
                                {
                                    if (ch == ' ') continue;
                                    if (ch != '&')
                                    {
                                        empty = false;
                                        break;
                                    }
                                }
                                if (empty)
                                {
                                    Plugin.logger.LogMessage("Forcing token cache recompute from " + region);
                                    forceRecomputeTokenCache = true;
                                    break;
                                }
                                continue;
                            }
                            Plugin.logger.LogMessage("Forcing token cache recompute from " + region);
                            forceRecomputeTokenCache = true;
                            break;
                        }

                        if (forceRecomputeTokenCache)
                        {
                            File.WriteAllText(text4 + Path.DirectorySeparatorChar + "recomputetokencache.txt", "");
                            flag2 = true;
                        }
                    }

                    return flag2;
                });
                c.Emit(OpCodes.Stloc, 16);
            }
            else Plugin.logger.LogError("InitializationScreen_UpdateIL failed spectacularly " + il);
        }

        private static void CollectiblesTracker_ctor(On.MoreSlugcats.CollectiblesTracker.orig_ctor orig, CollectiblesTracker self, Menu.Menu menu, MenuObject owner, UnityEngine.Vector2 pos, FContainer container, SlugcatStats.Name saveSlot)
        {
            orig.Invoke(self, menu, owner, pos, container, saveSlot);
            RainWorld rainWorld = menu.manager.rainWorld;
            for (int l = 0; l < self.displayRegions.Count; l++)
            {
                string region = self.displayRegions[l];
                if (regionCustomTokens.Keys.Count == 0) return;
                foreach (string key in regionCustomTokens.Keys)
                {
                    if (!regionCustomTokens.ContainsKey(key)) continue;
                    if (!regionCustomTokensAccessibility.ContainsKey(key)) continue;

                    if (CustomTokenDefinitions.TryGetValue(key, out var definition) &&
                        CustomTokenProgressions.TryGetValue(rainWorld.progression.currentSaveState.progression.miscProgressionData, out var progressions))
                    {
                        if (!regionCustomTokens[key].ContainsKey(region)) continue;
                        if (!regionCustomTokensAccessibility[key].ContainsKey(region)) continue;

                        for (int i = 0; i < regionCustomTokens[key][region].Count; i++)
                        {
                            if (regionCustomTokensAccessibility[key][region][i].Contains(saveSlot))
                            {
                                if (self.sprites.ContainsKey(region) && self.spriteColors.ContainsKey(region))
                                {
                                    string element = "ctOn";
                                    CustomTokenProgression progression = progressions.FirstOrDefault(x => x.tokenID.ToUpperInvariant() == key.ToUpperInvariant());
                                    if (progression == null || !progression.unlockedTokens.Contains(regionCustomTokens[key][region][i]))
                                    {
                                        element = "ctOff";
                                    }
                                    FSprite sprite = new FSprite(element, true);
                                    Color color = definition.tokenColor;
                                    sprite.color = color;
                                    self.spriteColors[region].Add(color);
                                    self.sprites[region].Add(sprite);

                                    container.AddChild(sprite);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void RainWorld_ReadTokenCacheIL(ILContext il)
        {
            ILCursor a = new(il); // Create new lists
            if (a.TryGotoNext(MoveType.Before,
                x => x.MatchLdcI4(7),
                x => x.MatchNewarr<string>(),
                x => x.MatchDup(),
                x => x.MatchLdcI4(0),
                x => x.MatchLdstr("World")
                ))
            {
                a.Emit(OpCodes.Ldloc_3);
                a.EmitDelegate<Action<string>>((text) =>
                {
                    foreach (string key in regionCustomTokens.Keys)
                    {
                        regionCustomTokens[key][text] = new();
                        regionCustomTokensAccessibility[key][text] = new();
                    }

                    string path = AssetManager.ResolveFilePath(string.Concat(new string[]
                    {
                        "World",
                        Path.DirectorySeparatorChar.ToString(),
                        "indexmaps",
                        Path.DirectorySeparatorChar.ToString(),
                        "lunacytokencache",
                        text,
                        ".txt"
                    }));
                    if (File.Exists(path))
                    {
                        string[] array = File.ReadAllText(path).Split('&');
                        for (int i = 0; i < array.Length; i++)
                        {
                            if (string.IsNullOrEmpty(array[i])) continue;
                            string[] tokens = array[i].Split(',');
                            for (int t = 0; t < tokens.Length; t++)
                            {
                                string[] keyValueAndSlugcats = tokens[t].Split('~');
                                int minusIndex = keyValueAndSlugcats[0].IndexOf('-');
                                string key = keyValueAndSlugcats[0].Substring(0, minusIndex);
                                string value = keyValueAndSlugcats[0].Substring(minusIndex + 1);
                                string[] slugcats = keyValueAndSlugcats[1].Split('|');

                                List<SlugcatStats.Name> list = [];
                                for (int s = 0; s < slugcats.Length; s++)
                                {
                                    list.Add(new SlugcatStats.Name(slugcats[s], false));
                                }
                                regionCustomTokens[key][text].Add(value);
                                regionCustomTokensAccessibility[key][text].Add(list);
                            }
                        }
                    }
                });
            }
            else Plugin.logger.LogError("RainWorld_ReadTokenCacheILA FAILURE!!!\n" + il);
        }

        private static void RainWorld_BuildTokenCacheIL(ILContext il)
        {
            ILCursor a = new(il); // Create new lists inside the lock
            if (a.TryGotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt("System.Threading.Monitor", "Enter")
                ))
            {
                a.Emit(OpCodes.Ldarg_2);
                a.EmitDelegate<Action<string>>((region) =>
                {
                    string fileName = region.ToLowerInvariant();

                    foreach (string key in regionCustomTokens.Keys)
                    {
                        regionCustomTokens[key][fileName] = new();
                        regionCustomTokensAccessibility[key][fileName] = new();
                    }
                });
            }
            else Plugin.logger.LogError("RainWorld_BuildTokenCacheA FAILURE!!!\n" + il);

            
            ILCursor b = new(il); // Check placed object and add to memory
            if (b.TryGotoNext(MoveType.Before,
                x => x.MatchLdloc(29),
                x => x.MatchLdfld<PlacedObject>("type"),
                x => x.MatchLdsfld(typeof(PlacedObject.Type).GetField(nameof(PlacedObject.Type.DataPearl)))
                ))
            {
                b.MoveAfterLabels();
                b.Emit(OpCodes.Ldloc, 29);
                b.Emit(OpCodes.Ldarg_2);
                b.Emit(OpCodes.Ldarg_0);
                b.Emit(OpCodes.Ldloc, 28);
                b.Emit(OpCodes.Ldloc, 12);
                b.EmitDelegate<Action<PlacedObject, string, RainWorld, List<SlugcatStats.Name>, List<SlugcatStats.Name>>>((obj, region, self, oldData, list3) =>
                {
                    string fileName = region.ToLowerInvariant();

                    foreach (string key in regionCustomTokens.Keys)
                    {
                        if (obj.type.value.ToLowerInvariant() == key.ToLowerInvariant() &&
                            CustomTokenDefinitions.TryGetValue(key, out var definition) && 
                            obj.data is CustomCollectTokenData data &&
                            definition.acceptableValues.Contains(data.tokenString))
                        {
                            if (!regionCustomTokens[key][fileName].Contains(data.tokenString))
                            {
                                regionCustomTokens[key][fileName].Add(data.tokenString);
                                regionCustomTokensAccessibility[key][fileName].Add(self.FilterTokenClearance(data.availableToPlayers, oldData, list3));
                            }
                            else
                            {
                                int index = regionCustomTokens[key][fileName].IndexOf(data.tokenString);
                                regionCustomTokensAccessibility[key][fileName][index] = self.FilterTokenClearance(data.availableToPlayers, regionCustomTokensAccessibility[key][fileName][index], list3);
                            }
                        }
                    }
                });
            }
            else Plugin.logger.LogError("RainWorld_BuildTokenCacheB FAILURE!!!\n" + il);
        }

        private static void RainWorld_BuildTokenCache(On.RainWorld.orig_BuildTokenCache orig, RainWorld self, bool modded, string region)
        {
            orig.Invoke(self, modded, region);

            string text = string.Concat(new string[]
            {
                Custom.RootFolderDirectory(),
                Path.DirectorySeparatorChar.ToString(),
                "World",
                Path.DirectorySeparatorChar.ToString(),
                "IndexMaps",
                Path.DirectorySeparatorChar.ToString()
            }).ToLowerInvariant();
            if (modded)
            {
                text = string.Concat(new string[]
                {
                    Custom.RootFolderDirectory(),
                    Path.DirectorySeparatorChar.ToString(),
                    "mergedmods",
                    Path.DirectorySeparatorChar.ToString(),
                    "World",
                    Path.DirectorySeparatorChar.ToString(),
                    "IndexMaps"
                }).ToLowerInvariant();
                Directory.CreateDirectory(text);
                text += Path.DirectorySeparatorChar.ToString();
            }
            string fileName = region.ToLowerInvariant();

            string contents = "";
            foreach (string key in regionCustomTokens.Keys)
            {
                for (int i = 0; i < regionCustomTokens[key][fileName].Count; i++)
                {
                    string str7 = string.Join("|", Array.ConvertAll<SlugcatStats.Name, string>(regionCustomTokensAccessibility[key][fileName][i].ToArray(), (SlugcatStats.Name x) => x.ToString()));
                    string keyTokenString = key + "-" + regionCustomTokens[key][fileName][i];
                    contents += ((keyTokenString != null) ? keyTokenString.ToString() : null) + "~" + str7;
                    if (i != regionCustomTokens[key][fileName].Count - 1)
                    {
                        contents += ",";
                    }
                }

                contents += "&";
            }
            if (contents.Length > 0) contents = contents.Substring(0, contents.Length - 1);
            if (contents != "" && contents != null) File.WriteAllText(text + "lunacytokencache" + fileName + ".txt", contents);
        }

        private static void RainWorld_ClearTokenCacheInMemory(On.RainWorld.orig_ClearTokenCacheInMemory orig, RainWorld self)
        {
            orig.Invoke(self);

            foreach (KeyValuePair<string, Dictionary<string, List<string>>> kvp in regionCustomTokens)
            {
                regionCustomTokens[kvp.Key].Clear();
                regionCustomTokensAccessibility[kvp.Key].Clear();
            }
        }

        private static void MiscProgressionData_FromString(On.PlayerProgression.MiscProgressionData.orig_FromString orig, PlayerProgression.MiscProgressionData self, string s)
        {
            orig.Invoke(self, s);

            if (!CustomTokenProgressions.TryGetValue(self, out _)) CustomTokenProgressions.Add(self, []);
            if (CustomTokenProgressions.TryGetValue(self, out var progressions))
            {
                foreach (var text in self.unrecognizedSaveStrings)
                {
                    try
                    {
                        List<string> data = Regex.Split(text, "<mpdB>").ToList();
                        if (data == null || data.Count == 0) continue;

                        if (data[0].StartsWith("LUNACYTOKEN_"))
                        {
                            CustomTokenProgression progression = new CustomTokenProgression(data[0].Substring(12));
                            data.RemoveAt(0);
                            progression.unlockedTokens = data;
                            progressions.Add(progression);

                            Plugin.logger.LogInfo($"Succesfully parsed custom token data from {text}! TokenID: {progression.tokenID}, UnlockedTokens: {string.Join(", ", progression.unlockedTokens)}");
                        }
                    }
                    catch (Exception e)
                    {
                        Plugin.logger.LogError("FAILED TO LOAD CUSTOM TOKEN DATA FROM STRING\n" + e);
                    }
                }
            }
        }

        private static string MiscProgressionData_ToString(On.PlayerProgression.MiscProgressionData.orig_ToString orig, PlayerProgression.MiscProgressionData self)
        {
            if (CustomTokenProgressions.TryGetValue(self, out var progressions))
            {
                if (progressions.Count != 0)
                {
                    foreach (var data in progressions)
                    {
                        string saveDataKey = "LUNACYTOKEN_" + data.tokenID.ToUpperInvariant();
                        string remove = self.unrecognizedSaveStrings.Find(x => x.StartsWith(saveDataKey));
                        if (remove != null) self.unrecognizedSaveStrings.Remove(remove);

                        string text = "<mpdB>" + data.ToString();

                        self.unrecognizedSaveStrings.Add(saveDataKey + text);
                    }
                }
            }

            return orig.Invoke(self);
        }

        private static void MiscProgressionData_ctor(On.PlayerProgression.MiscProgressionData.orig_ctor orig, PlayerProgression.MiscProgressionData self, PlayerProgression owner)
        {
            orig.Invoke(self, owner);

            if (!CustomTokenProgressions.TryGetValue(self, out var progression)) CustomTokenProgressions.Add(self, []);
            else progression.Clear();
        }

        private static ObjectsPage.DevObjectCategories ObjectsPage_DevObjectGetCategoryFromPlacedType(On.DevInterface.ObjectsPage.orig_DevObjectGetCategoryFromPlacedType orig, ObjectsPage self, PlacedObject.Type type)
        {
            foreach (KeyValuePair<string, CustomTokenDefinition> kvp in CustomTokenDefinitions)
            {
                if (type.value == kvp.Key) return new(kvp.Value.devtoolsCategory, false);
            }
            return orig.Invoke(self, type);
        }

        private static void PlacedObject_GenerateEmptyData(On.PlacedObject.orig_GenerateEmptyData orig, PlacedObject self)
        {
            foreach (string key in CustomTokenDefinitions.Keys)
            {
                if (self.type.value == key)
                {
                    self.data = new CustomCollectTokenData(self, key);
                    return;
                }
            }
            orig.Invoke(self);
        }

        private static void ObjectsPage_CreateObjRep(On.DevInterface.ObjectsPage.orig_CreateObjRep orig, ObjectsPage self, PlacedObject.Type tp, PlacedObject pObj)
        {
            orig.Invoke(self, tp, pObj);

            if (pObj != null)
            {
                foreach (KeyValuePair<string, CustomTokenDefinition> kvp in CustomTokenDefinitions)
                {
                    if (tp.value == kvp.Key)
                    {
                        PlacedObjectRepresentation rep = new CustomTokenRepresentation(self.owner, tp.ToString(), self, pObj, kvp.Value);
                        if (rep != null)
                        {
                            self.tempNodes.Add(rep);
                            self.subNodes.Add(rep);
                        }
                    }
                }
            }
        }

        public static void AddCustomToken(string tokenID, CustomTokenDefinition definition)
        {
            if (tokenID.Contains('-'))
            {
                Plugin.logger.LogError($"TokenIDs aren't allowed to have a \"-\" in their name!");
                return;
            }
            if (tokenID.Contains('~'))
            {
                Plugin.logger.LogError($"TokenIDs aren't allowed to have a \"~\" in their name!");
                return;
            }
            if (tokenID.Contains('|'))
            {
                Plugin.logger.LogError($"TokenIDs aren't allowed to have a \"|\" in their name!");
                return;
            }
            if (tokenID.Contains(','))
            {
                Plugin.logger.LogError($"TokenIDs aren't allowed to have a \",\" in their name!");
                return;
            }
            if (CustomTokenDefinitions.ContainsKey(tokenID))
            {
                Plugin.logger.LogError($"Custom token with tokenID {tokenID} already exists!");
                return;
            }
            CustomTokenDefinitions.Add(tokenID, definition);
            definition.tokenID = tokenID;
        }

        public static bool GetCustomTokenCollected(PlayerProgression.MiscProgressionData miscProgressionData, string tokenID, string collectible)
        {
            if (CustomTokenProgressions.TryGetValue(miscProgressionData, out var progressions))
            {
                foreach (var progression in progressions)
                {
                    if (progression.unlockedTokens.Count > 0 && progression.tokenID.ToUpperInvariant() == tokenID.ToUpperInvariant())
                    {
                        return progression.unlockedTokens.Contains(collectible);
                    }
                }
            } 

            return false;
        }

        public static bool SetCustomTokenCollected(PlayerProgression.MiscProgressionData miscProgressionData, string tokenID, string collectible)
        {
            if (GetCustomTokenCollected(miscProgressionData, tokenID, collectible)) return false;
            if (CustomTokenProgressions.TryGetValue(miscProgressionData, out var progressions))
            {
                foreach (var progression in progressions)
                {
                    if (progression.tokenID.ToUpperInvariant() == tokenID.ToUpperInvariant())
                    {
                        progression.unlockedTokens.Add(collectible);
                        return true;
                    }
                }
                // progressions doesnt have this tokenID
                CustomTokenProgression newProgression = new CustomTokenProgression(tokenID.ToUpperInvariant());
                newProgression.unlockedTokens.Add(collectible);
                progressions.Add(newProgression);
                return true;
            }
            // progressions doesnt have a list
            CustomTokenProgression newerProgression = new CustomTokenProgression(tokenID.ToUpperInvariant());
            newerProgression.unlockedTokens.Add(collectible);
            CustomTokenProgressions.Add(miscProgressionData, [newerProgression]);
            return true;
        }

        public static CustomTokenProgression GetCustomTokenProgression(PlayerProgression.MiscProgressionData miscProgressionData, string tokenID)
        {
            if (CustomTokenProgressions.TryGetValue(miscProgressionData, out var progressions))
            {
                foreach (var progression in progressions)
                {
                    if (progression.tokenID.ToUpperInvariant() == tokenID.ToUpperInvariant())
                    {
                        return progression;
                    }
                }
            }
            return null;
        }
    }
}
