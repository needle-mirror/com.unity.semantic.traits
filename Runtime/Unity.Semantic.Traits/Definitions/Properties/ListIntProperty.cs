using System;
using System.Collections.Generic;

namespace Unity.Semantic.Traits
{
    /// <summary>
    /// List of integer property type definition
    /// </summary>
    [Serializable]
    [TraitPropertyType("List/Integer")]
    public class ListIntProperty : TraitPropertyDefinition
    {
        /// <inheritdoc />
        public override Type Type => typeof(List<int>);
    }
}
