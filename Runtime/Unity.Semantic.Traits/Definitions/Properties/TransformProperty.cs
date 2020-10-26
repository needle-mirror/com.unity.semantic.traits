using System;
using UnityEngine;

namespace Unity.Semantic.Traits
{
    /// <summary>
    /// Transform property type definition
    /// </summary>
    [Serializable]
    public class TransformProperty : TraitPropertyDefinition
    {
        /// <inheritdoc />
        public override Type Type => typeof(Transform);
    }
}
