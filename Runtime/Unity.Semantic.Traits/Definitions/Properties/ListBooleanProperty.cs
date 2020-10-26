using System;
using System.Collections.Generic;

namespace Unity.Semantic.Traits
{
    /// <summary>
    /// List of boolean property type definition
    /// </summary>
    [Serializable]
    [TraitPropertyType("List/Boolean")]
    public class ListBooleanProperty : TraitPropertyDefinition
    {
        /// <inheritdoc />
        public override Type Type => typeof(List<bool>);
    }
}
