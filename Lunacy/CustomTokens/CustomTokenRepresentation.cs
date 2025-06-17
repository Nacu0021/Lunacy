using DevInterface;
using System;
using UnityEngine;
using MoreSlugcats;
using RWCustom;

namespace Lunacy.CustomTokens
{
    // Copied from vanilla game and modified
    public class CustomTokenRepresentation : ResizeableObjectRepresentation
    {
        private int lineSprite;

        public CustomTokenRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, CustomTokenDefinition definition) : base(owner, IDstring, parentNode, pObj, IDstring, false)
        {
            this.subNodes.Add(new CustomTokenControlPanel(owner, "Token_Panel", this, new Vector2(0f, 100f), definition));
            (this.subNodes[this.subNodes.Count - 1] as CustomTokenControlPanel).pos = (pObj.data as CustomCollectTokenData).panelPos;
            this.fSprites.Add(new FSprite("pixel", true));
            this.lineSprite = this.fSprites.Count - 1;
            owner.placedObjectsContainer.AddChild(this.fSprites[this.lineSprite]);
            this.fSprites[this.lineSprite].anchorY = 0f;
        }

        public override void Refresh()
        {
            base.Refresh();
            base.MoveSprite(this.lineSprite, this.absPos);
            this.fSprites[this.lineSprite].scaleY = (this.subNodes[1] as CustomTokenControlPanel).pos.magnitude;
            this.fSprites[this.lineSprite].rotation = Custom.AimFromOneVectorToAnother(this.absPos, (this.subNodes[1] as CustomTokenControlPanel).absPos);
            (this.pObj.data as CustomCollectTokenData).panelPos = (this.subNodes[1] as Panel).pos;
        }

        public class CustomTokenControlPanel : Panel, IDevUISignals
        {
            public Button[] buttons;
            public DevUILabel lbl;

            public CustomCollectTokenData TokenData
            {
                get
                {
                    return (this.parentNode as CustomTokenRepresentation).pObj.data as CustomCollectTokenData;
                }
            }

            public CustomTokenControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, CustomTokenDefinition definition) : base(owner, IDstring, parentNode, pos, new Vector2(250f, 100f), "Collectable Token")
            {
                this.lbl = new DevUILabel(owner, "Token_Label", this, new Vector2(5f, 75f), 240f, "");
                this.subNodes.Add(this.lbl);
                this.subNodes.Add(new CustomTokenControlPanel.IndexControlSlider(owner, "Index_Slider", this, new Vector2(5f, 55f), "Token Index: ", definition));
                this.buttons = new Button[ExtEnum<SlugcatStats.Name>.values.Count];
                float num = 10f;
                float num2 = 20f;
                for (int i = 0; i < this.buttons.Length; i++)
                {
                    this.buttons[i] = new Button(owner, "Button_" + i.ToString(), this, new Vector2(5f + (num2 + 5f) * ((float)i % num), 5f + (float)((int)((float)i / num)) * (num2 + 5f)), num2, "");
                    this.subNodes.Add(this.buttons[i]);
                }
                this.UpdateButtonText();
                this.UpdateTokenText();
            }

            private void UpdateTokenText()
            {
                if (string.IsNullOrEmpty(TokenData.tokenString))
                {
                    lbl.Text = "Undefined Value";
                    return;
                }
                lbl.Text = TokenData.tokenString;
            }

            private void UpdateButtonText()
            {
                for (int i = 0; i < this.buttons.Length; i++)
                {
                    SlugcatStats.Name name = new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(i), false);
                    if (this.TokenData.availableToPlayers.Contains(name))
                    {
                        this.buttons[i].Text = name.value.Substring(0, 2);
                    }
                    else
                    {
                        this.buttons[i].Text = "--";
                    }
                }
            }

            public void Signal(DevUISignalType type, DevUINode sender, string message)
            {
                for (int i = 0; i < this.buttons.Length; i++)
                {
                    if (this.buttons[i] == sender)
                    {
                        SlugcatStats.Name item = new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(i), false);
                        if (((this.parentNode as CustomTokenRepresentation).pObj.data as CustomCollectTokenData).availableToPlayers.Contains(item))
                        {
                            ((this.parentNode as CustomTokenRepresentation).pObj.data as CustomCollectTokenData).availableToPlayers.Remove(item);
                        }
                        else
                        {
                            ((this.parentNode as CustomTokenRepresentation).pObj.data as CustomCollectTokenData).availableToPlayers.Add(item);
                        }
                    }
                }
                this.UpdateButtonText();
            }

            public class IndexControlSlider : Slider
            {
                public int maxNubInt;
                public CustomTokenDefinition definition;

                public CustomCollectTokenData TokenData
                {
                    get
                    {
                        return (this.parentNode.parentNode as CustomTokenRepresentation).pObj.data as CustomCollectTokenData;
                    }
                }

                public IndexControlSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title, CustomTokenDefinition definition) : base(owner, IDstring, parentNode, pos, title, false, 110f)
                {
                    this.definition = definition;
                    maxNubInt = definition.acceptableValues.Count - 1;
                }

                public override void Refresh()
                {
                    base.Refresh();
                    int num = 0;
                    if (IDstring != null && IDstring == "Index_Slider")
                    {
                        num = Mathf.Max(0, definition.acceptableValues.IndexOf(TokenData.tokenString));
                    }
                    base.NumberText = num.ToString();
                    base.RefreshNubPos((float)num / (float)this.maxNubInt);
                }

                public override void NubDragged(float nubPos)
                {
                    if (IDstring != null && IDstring == "Index_Slider")
                    {
                        TokenData.tokenString = definition.acceptableValues[Mathf.FloorToInt(nubPos * maxNubInt)];
                    }
                    parentNode.parentNode.Refresh();
                    (parentNode as CustomTokenControlPanel).UpdateTokenText();
                    Refresh();
                }
            }
        }
    }
}
