using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
class TraitSelectorAttribute : PropertyAttribute
{
    public enum TraitFilter
    {
        Required,
        Prohibited
    }

    public TraitFilter Filter { get; }
    public Type DisplayTraitType { get; }

    public TraitSelectorAttribute(TraitFilter filter, Type displayTraitType = null)
    {
        Filter = filter;
        DisplayTraitType = displayTraitType;
    }
}
