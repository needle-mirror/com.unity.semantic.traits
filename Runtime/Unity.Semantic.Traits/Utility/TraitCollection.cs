using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Semantic.Traits
{
    [Serializable]
    struct TraitCollection
    {
        public int Count => m_Traits.Count;

        public TraitDefinition this[int index] => m_Traits[index];

#pragma warning disable 0649
        [SerializeField]
        List<TraitDefinition> m_Traits;
#pragma warning restore 0649

        internal void Add(TraitDefinition definition)
        {
            if (!m_Traits.Contains(definition))
                m_Traits.Add(definition);
        }

        internal void Clear()
        {
            m_Traits.Clear();
        }
    }
}
