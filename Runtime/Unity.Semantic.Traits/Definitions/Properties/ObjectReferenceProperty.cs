using System;
using Unity.Entities;
using UnityEngine;

namespace Unity.Semantic.Traits
{
    /// <summary>
    /// Object reference property type definition
    /// </summary>
    [Serializable]
    [TraitPropertyType("Object Reference")]
    public class ObjectReferenceProperty : TraitPropertyDefinition
    {
        /// <inheritdoc />
        public override Type Type => typeof(GameObject);
    }
}
