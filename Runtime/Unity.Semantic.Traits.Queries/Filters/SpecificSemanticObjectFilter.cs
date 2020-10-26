using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

namespace Unity.Semantic.Traits.Queries
{
    /// <summary>
    /// Filter that checks for semantic object on a specific object
    /// </summary>
    [Serializable]
    [QueryEditor("Specific Object", "Object is [m_ReferenceObject]")]
    public struct SpecificSemanticObjectFilter : IQueryFilter
    {
        /// <summary>
        /// Reference object used to validate the result
        /// </summary>
        public SemanticObject ReferenceObject
        {
            get => m_ReferenceObject;
            set => m_ReferenceObject = value;
        }

#pragma warning disable 0649
        [SerializeField]
        SemanticObject m_ReferenceObject;
#pragma warning restore 0649

        /// <inheritdoc />
        public void Validate(EntityManager entityManager, NativeArray<Entity> entities, UnsafeBitArray entitiesValid)
        {
            if (m_ReferenceObject == null || m_ReferenceObject.Entity == default)
                return;

            for (var i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];

                // Only check entities that haven't already failed queries
                if (entitiesValid.IsSet(i))
                {
                    entitiesValid.Set(i, entity == m_ReferenceObject.Entity);
                }
            }
        }
    }
}
