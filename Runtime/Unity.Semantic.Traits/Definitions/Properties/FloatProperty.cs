using System;
using UnityEngine;

namespace Unity.Semantic.Traits
{
    /// <summary>
    /// Float property type definition
    /// </summary>
    [Serializable]
    [TraitPropertyType("Float")]
    public class FloatProperty : TraitPropertyDefinition
    {
#pragma warning disable 0649
        [SerializeField]
        float m_Value;
#pragma warning restore 0649

        /// <inheritdoc />
        public override Type Type => typeof(float);

        /// <summary>
        /// Float value
        /// </summary>
        public float Value
        {
            get => m_Value;
            set => m_Value = value;
        }
    }
}
