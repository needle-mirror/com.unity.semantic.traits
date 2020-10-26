using System;
using UnityEngine;

namespace Unity.Semantic.Traits
{
    /// <summary>
    /// Boolean property type definition
    /// </summary>
    [Serializable]
    [TraitPropertyType("Boolean")]
    public class BooleanProperty : TraitPropertyDefinition
    {
#pragma warning disable 0649
        [SerializeField]
        bool m_Value;
#pragma warning restore 0649

        /// <inheritdoc />
        public override Type Type => typeof(bool);

        /// <summary>
        /// Boolean value
        /// </summary>
        public bool Value
        {
            get => m_Value;
            set => m_Value = value;
        }
    }
}
