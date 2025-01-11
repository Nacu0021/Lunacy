using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Lunacy.CustomTokens
{
    // Copied from vanilla game and modified
    public class CustomCollectTokenData : PlacedObject.ResizableObjectData
    {
        public Vector2 panelPos;
        public string tokenString;
        public string customTokenID;
        public List<SlugcatStats.Name> availableToPlayers;

        public CustomCollectTokenData(PlacedObject owner, string customTokenID) : base(owner)
        {
            this.availableToPlayers = new List<SlugcatStats.Name>();
            for (int i = 0; i < ExtEnum<SlugcatStats.Name>.values.Count; i++)
            {
                SlugcatStats.Name name = new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(i), false);
                if (!SlugcatStats.HiddenOrUnplayableSlugcat(name))
                {
                    this.availableToPlayers.Add(name);
                }
            }

            this.customTokenID = customTokenID;
        }

        public override void FromString(string s)
        {
            string[] array = Regex.Split(s, "~");
            this.handlePos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
            this.handlePos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
            this.panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
            this.panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
            this.tokenString = array[4];
            this.customTokenID = array[5];
            if (Custom.IsDigitString(array[6]))
            {
                BackwardsCompatibilityRemix.ParsePlayerAvailability(array[6], this.availableToPlayers);
            }
            else
            {
                this.availableToPlayers.Clear();
                List<string> list = new List<string>(array[6].Split(new char[]
                {
                '|'
                }));
                for (int i = 0; i < ExtEnum<SlugcatStats.Name>.values.Count; i++)
                {
                    string entry = ExtEnum<SlugcatStats.Name>.values.GetEntry(i);
                    SlugcatStats.Name name = new SlugcatStats.Name(entry, false);
                    if (!SlugcatStats.HiddenOrUnplayableSlugcat(name) && !list.Contains(entry))
                    {
                        this.availableToPlayers.Add(name);
                    }
                }
            }
            this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 7);
        }

        public override string ToString()
        {
            List<string> list = new List<string>();
            for (int i = 0; i < ExtEnum<SlugcatStats.Name>.values.Count; i++)
            {
                string entry = ExtEnum<SlugcatStats.Name>.values.GetEntry(i);
                SlugcatStats.Name name = new SlugcatStats.Name(entry, false);
                if (!SlugcatStats.HiddenOrUnplayableSlugcat(name) && !this.availableToPlayers.Contains(name))
                {
                    list.Add(entry);
                }
            }
            string text = string.Join("|", list.ToArray());
            string text2 = string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}", new object[]
            {
            this.handlePos.x,
            this.handlePos.y,
            this.panelPos.x,
            this.panelPos.y,
            this.tokenString,
            this.customTokenID,
            text
            });
            text2 = SaveState.SetCustomData(this, text2);
            return SaveUtils.AppendUnrecognizedStringAttrs(text2, "~", this.unrecognizedAttributes);
        }
    }
}
