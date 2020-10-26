using System;
using System.Collections.Generic;

namespace Unity.Semantic.Traits
{
    /// <summary>
    /// List of float property type definition
    /// </summary>
    [Serializable]
    [TraitPropertyType("List/Float")]
    public class ListFloatProperty : TraitPropertyDefinition
    {
        /// <inheritdoc />
        public override Type Type => typeof(List<float>);
    }
}
