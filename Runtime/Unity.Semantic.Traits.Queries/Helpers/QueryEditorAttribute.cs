using System;

namespace Unity.Semantic.Traits.Queries
{
    /// <summary>
    /// Tells the SemanticQuery editor how to display a Filter or a Scorer container
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public sealed class QueryEditorAttribute : Attribute
    {
        internal string Name { get; }
        internal string Description { get; }
        internal Type RequiredTraitData { get; }

        /// <summary>
        /// Defines a query filter editor
        /// </summary>
        /// <param name="name">Name of the filter</param>
        /// <param name="descriptionFormat">Format the filter and its parameters</param>
        /// <param name="requiredTraitData">Optional trait requirement</param>
        public QueryEditorAttribute(string name, string descriptionFormat, Type requiredTraitData = null)
        {
            Name = name;
            Description = descriptionFormat;
            RequiredTraitData = requiredTraitData;
        }
    }
}
