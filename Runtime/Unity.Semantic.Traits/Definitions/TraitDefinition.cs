using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.Semantic.Traits
{
    /// <summary>
    /// Definition of a trait that specify the quality of an object
    /// </summary>
    [CreateAssetMenu(fileName = "NewTrait", menuName = Constants.MenuName + "/Trait Definition")]
    public class TraitDefinition : ScriptableObject
    {
        [SerializeField]
        Color m_Color = new Color(0.28f, 1, 1);

        [SerializeReference]
        List<TraitPropertyDefinition> m_Properties = new List<TraitPropertyDefinition>();

        [SerializeField]
        int m_NextElementId;

        internal new string name
        {
            get
            {
                var baseName = base.name;
                if (string.IsNullOrEmpty(m_BaseName) || m_BaseName != baseName)
                {
                    // Cache because it is expensive to check this every frame
                    m_BaseName = baseName;
                    m_TypeName = Utility.TypeResolver.ToTypeNameCase(baseName);
                }

                return m_TypeName;
            }
        }

        internal IEnumerable<TraitPropertyDefinition> Properties
        {
            get => m_Properties;
            set => m_Properties = value.ToList();
        }

        internal Color Color
        {
            get => m_Color;
            set => m_Color = value;
        }

        string m_BaseName;
        string m_TypeName;

        internal TraitPropertyDefinition GetProperty(int propertyId)
        {
            return m_Properties.FirstOrDefault(p => p.Id == propertyId);
        }
    }
}
