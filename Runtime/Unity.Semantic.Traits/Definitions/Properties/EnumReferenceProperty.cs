using System;
using Unity.Semantic.Traits.Utility;
using UnityEngine;

namespace Unity.Semantic.Traits
{
    /// <summary>
    /// Enum reference property type definition
    /// </summary>
    [Serializable]
    public class EnumReferenceProperty : TraitPropertyDefinition
    {
#pragma warning disable 0649
        [SerializeField]
        EnumDefinition m_EnumReference;

        [SerializeField]
        int m_Value;
#pragma warning restore 0649

        /// <inheritdoc />
        public override Type Type => typeof(Enum);

        /// <summary>
        /// Enumeration asset reference
        /// </summary>
        public EnumDefinition Reference
        {
            get { return m_EnumReference; }
            set { m_EnumReference = value; }
        }

        /// <summary>
        /// Unique Id of the enumeration value
        /// </summary>
        public int Value
        {
            get => m_Value;
            set => m_Value = value;
        }
    }
}
