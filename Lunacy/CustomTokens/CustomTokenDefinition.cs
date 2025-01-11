using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lunacy.CustomTokens
{
    // Copied from vanilla game and modified
    public class CustomTokenDefinition
    {
        public string tokenID;
        public Color tokenColor;
        public List<string> acceptableValues;
        public Func<Player, CustomCollectToken, string, Action<CustomCollectToken>> collectCallback;
        public string devtoolsCategory;

        public CustomTokenDefinition(Color tokenColor, List<string> acceptableValues, Func<Player, CustomCollectToken, string, Action<CustomCollectToken>> collectCallback, string devtoolsCategory = "Tutorial")
        {
            this.tokenColor = tokenColor;
            this.acceptableValues = acceptableValues;
            this.collectCallback = collectCallback;
            this.devtoolsCategory = devtoolsCategory;
        }
    }
}
