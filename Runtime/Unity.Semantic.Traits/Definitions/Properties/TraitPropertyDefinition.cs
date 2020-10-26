using System;
using UnityEngine;

namespace Unity.Semantic.Traits
{
    /// <summary>
    /// Base definition class used to declare a property type that can be used in a trait
    /// </summary>
    [Serializable]
    public abstract class TraitPropertyDefinition : INamedProperty
    {
#pragma warning disable 0649
        [SerializeField]
        string m_Name;

        [SerializeField]
        int m_Id;
#pragma warning restore 0649

        /// <summary>
        /// Property unique Id
        /// </summary>
        public int Id
        {
            get => m_Id;
            set => m_Id = value;
        }

        /// <summary>
        /// Property Name
        /// </summary>
        public string Name
        {
            get => m_Name;
            set => m_Name = value;
        }

        /// <summary>
        /// Type of data stored by the definition
        /// </summary>
        public abstract Type Type { get; }
    }
}
