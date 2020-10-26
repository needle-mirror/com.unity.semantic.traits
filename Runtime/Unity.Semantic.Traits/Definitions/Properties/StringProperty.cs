using System;
using Unity.Collections;
using UnityEngine;

namespace Unity.Semantic.Traits
{
    /// <summary>
    /// String property type definition
    /// </summary>
    [Serializable]
    [TraitPropertyType("String")]
    public class StringProperty : TraitPropertyDefinition
    {
#pragma warning disable 0649
        [SerializeField]
        string m_Value;
#pragma warning restore 0649

        /// <inheritdoc />
        public override Type Type => typeof(string);

        /// <summary>
        /// String value
        /// </summary>
        public string Value
        {
            get => m_Value;
            set => m_Value = value;
        }
    }
}
