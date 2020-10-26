using System;
using Unity.Mathematics;
using UnityEngine;

namespace Unity.Semantic.Traits
{
    /// <summary>
    /// Vector2 property type definition
    /// </summary>
    [Serializable]
    [TraitPropertyType("Vector2")]
    public class Vector2Property : TraitPropertyDefinition
    {
#pragma warning disable 0649
        [SerializeField]
        Vector2 m_Value;
#pragma warning restore 0649

        /// <inheritdoc />
        public override Type Type => typeof(Vector2);

        /// <summary>
        /// Vector2 value
        /// </summary>
        public Vector2 Value
        {
            get => m_Value;
            set => m_Value = value;
        }
    }
}
