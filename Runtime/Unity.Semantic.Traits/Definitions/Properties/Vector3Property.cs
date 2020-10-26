using System;
using Unity.Mathematics;
using UnityEngine;

namespace Unity.Semantic.Traits
{
    /// <summary>
    /// Vector3 property type definition
    /// </summary>
    [Serializable]
    [TraitPropertyType("Vector3")]
    public class Vector3Property : TraitPropertyDefinition
    {
#pragma warning disable 0649
        [SerializeField]
        Vector3 m_Value;
#pragma warning restore 0649

        /// <inheritdoc />
        public override Type Type => typeof(Vector3);

        /// <summary>
        /// Vector3 value
        /// </summary>
        public Vector3 Value
        {
            get => m_Value;
            set => m_Value = value;
        }
    }
}
