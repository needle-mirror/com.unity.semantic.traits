using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Semantic.Traits.Utility;
using UnityEngine;

namespace Unity.Semantic.Traits
{
    [Serializable]
    class EnumElementDefinition : INamedProperty
    {
        [SerializeField]
        int m_Id;

        [SerializeField]
        string m_Name;

        public EnumElementDefinition(string name, int id)
        {
            m_Name = name;
            m_Id = id;
        }

        public int Id => m_Id;

        public string Name
        {
            get => m_Name;
            set => m_Name = value;
        }
    }

    /// <summary>
    /// Definition of an enumeration
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnumeration", menuName = Constants.MenuName + "/Enum Definition")]
    public class EnumDefinition : ScriptableObject
    {
        [SerializeField]
        List<EnumElementDefinition> m_Elements = new List<EnumElementDefinition>();

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
                    m_TypeName = TypeResolver.ToTypeNameCase(baseName);
                }

                return m_TypeName;
            }
        }

        string m_BaseName;
        string m_TypeName;

        internal IEnumerable<EnumElementDefinition> Elements
        {
            get => m_Elements;
            set => m_Elements = value.ToList();
        }

        internal string GetElementNameById(int id)
        {
            return m_Elements.Find(a => a.Id == id)?.Name;
        }
    }
}
