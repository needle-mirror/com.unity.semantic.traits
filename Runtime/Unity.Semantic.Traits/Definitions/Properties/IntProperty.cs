using System;
using UnityEngine;

namespace Unity.Semantic.Traits
{
    /// <summary>
    /// Integer property type definition
    /// </summary>
    [Serializable]
    [TraitPropertyType("Integer")]
    public class IntProperty : TraitPropertyDefinition
    {
#pragma warning disable 0649
        [SerializeField]
        int m_Value;
#pragma warning restore 0649

        /// <inheritdoc />
        public override Type Type => typeof(int);

        /// <summary>
        /// Integer value
        /// </summary>
        public int Value
        {
            get => m_Value;
            set => m_Value = value;
        }
    }
}
