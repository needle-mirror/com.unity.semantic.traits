using Unity.Semantic.Traits.Utility;
using UnityEngine;

namespace UnityEditor.Semantic.Traits.UI
{
    static class EditorStyleHelper
    {
        public static GUIStyle requiredTraitLabel = new GUIStyle("AssetLabel");
        public static GUIStyle prohibitedTraitLabel = new GUIStyle("AssetLabel");
        public static GUIStyle requiredTraitAdd = new GUIStyle(requiredTraitLabel);
        public static GUIStyle prohibitedTraitAdd = new GUIStyle(prohibitedTraitLabel);
        public static GUIStyle requiredTraitMore = new GUIStyle(requiredTraitLabel);
        public static GUIStyle prohibitedTraitMore = new GUIStyle(prohibitedTraitLabel);

        static EditorStyleHelper()
        {
            var traitResources = TraitResources.instance;
            requiredTraitLabel.normal.background = traitResources.ImageRequiredTraitLabel;
            requiredTraitLabel.normal.scaledBackgrounds = null;
            prohibitedTraitLabel.normal.background = traitResources.ImageProhibitedTraitLabel;
            prohibitedTraitLabel.normal.scaledBackgrounds = null;
            requiredTraitAdd.normal.background = traitResources.ImageRequiredTraitAdd;
            requiredTraitAdd.normal.scaledBackgrounds = null;
            prohibitedTraitAdd.normal.background = traitResources.ImageProhibitedTraitAdd;
            prohibitedTraitAdd.normal.scaledBackgrounds = null;
            requiredTraitMore.normal.background = traitResources.ImageRequiredTraitMore;
            requiredTraitMore.normal.scaledBackgrounds = null;
            prohibitedTraitMore.normal.background = traitResources.ImageProhibitedTraitMore;
            prohibitedTraitMore.normal.scaledBackgrounds = null;
        }
    }
}
