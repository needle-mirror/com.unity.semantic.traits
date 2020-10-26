using System;
using UnityEngine;

namespace Unity.Semantic.Traits
{
    /// <summary>
    /// Use this attribute on TraitPropertyDefinition to specify the display name shown in the Inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TraitPropertyTypeAttribute : PropertyAttribute
    {
        /// <summary>
        /// Name to display in the Inspector.
        /// </summary>
        public readonly string displayName;

        /// <summary>
        /// Specify a display name.
        /// </summary>
        /// <param name="displayName">The name to display.</param>
        public TraitPropertyTypeAttribute(string displayName)
        {
            this.displayName = displayName;
        }
    }
}
