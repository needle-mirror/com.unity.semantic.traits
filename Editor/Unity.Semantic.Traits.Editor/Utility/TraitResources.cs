using UnityEditor;
using UnityEngine;

namespace Unity.Semantic.Traits.Utility
{
    class TraitResources : ScriptableSingleton<TraitResources>
    {
        public Texture2D ImageRequiredTraitLabel => m_ImageRequiredTraitLabel;
        public Texture2D ImageRequiredTraitAdd => m_ImageRequiredTraitAdd;
        public Texture2D ImageRequiredTraitMore => m_ImageRequiredTraitMore;
        public Texture2D ImageProhibitedTraitLabel => m_ImageProhibitedTraitLabel;
        public Texture2D ImageProhibitedTraitAdd => m_ImageProhibitedTraitAdd;
        public Texture2D ImageProhibitedTraitMore => m_ImageProhibitedTraitMore;

        public TextAsset TemplateEnum => m_TemplateEnum;
        public TextAsset TemplateTraitData => m_TemplateTraitData;
        public TextAsset TemplateTrait => m_TemplateTrait;
        public TextAsset TemplatePackage => m_TemplatePackage;
        public TextAsset TemplateAsmRef => m_TemplateAsmRef;

#pragma warning disable 0649

        [SerializeField]
        Texture2D m_ImageRequiredTraitLabel;
        [SerializeField]
        Texture2D m_ImageRequiredTraitAdd;
        [SerializeField]
        Texture2D m_ImageRequiredTraitMore;
        [SerializeField]
        Texture2D m_ImageProhibitedTraitLabel;
        [SerializeField]
        Texture2D m_ImageProhibitedTraitAdd;
        [SerializeField]
        Texture2D m_ImageProhibitedTraitMore;
        [SerializeField]
        Texture2D m_ImageTraitNode;

        [SerializeField]
        TextAsset m_TemplateEnum;
        [SerializeField]
        TextAsset m_TemplateTrait;
        [SerializeField]
        TextAsset m_TemplateTraitData;
        [SerializeField]
        TextAsset m_TemplatePackage;
        [SerializeField]
        TextAsset m_TemplateAsmRef;

#pragma warning restore 0649
    }
}
