using System;
using System.Linq;
using UnityEngine;

namespace Unity.Semantic.Traits
{
    [Serializable]
    struct TraitPropertyHandle : IEquatable<TraitPropertyHandle>
    {
        [SerializeField]
        TraitDefinition m_Trait;

        [SerializeField]
        int m_PropertyId;

        public TraitPropertyHandle(TraitDefinition trait, int traitPropertyId)
        {
            m_Trait = trait;
            m_PropertyId = traitPropertyId;
        }

        internal string Name
        {
            get
            {
                if (m_Trait == null || m_PropertyId < 0)
                    return string.Empty;

                return m_Trait.GetProperty(m_PropertyId)?.Name;
            }
        }

        internal Type Type
        {
            get
            {
                if (m_Trait == null || m_PropertyId < 0)
                    return null;

                return m_Trait.GetProperty(m_PropertyId)?.Type;
            }
        }

        internal TraitPropertyDefinition GetPropertyDefinition()
        {
            return m_Trait.GetProperty(m_PropertyId);
        }

        internal TraitDefinition Trait => m_Trait;

        internal int PropertyId => m_PropertyId;

        public bool Equals(TraitPropertyHandle other)
        {
            return Equals(m_Trait, other.m_Trait) && m_PropertyId == other.m_PropertyId;
        }

        public override bool Equals(object obj)
        {
            return obj is TraitPropertyHandle other && Equals(other);
        }

        public static bool operator ==(TraitPropertyHandle h1, TraitPropertyHandle h2)
        {
            return h1.Equals(h2);
        }

        public static bool operator !=(TraitPropertyHandle h1, TraitPropertyHandle h2)
        {
            return !h1.Equals(h2);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((m_Trait != null ? m_Trait.GetHashCode() : 0) * 397) ^ m_PropertyId;
            }
        }
    }
}
