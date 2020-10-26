using System;
using Unity.Semantic.Traits;
using Unity.Semantic.Traits.Queries;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

namespace Unity.Semantic.Traits.Queries
{
    /// <summary>
    /// Filter that checks for every semantic objects in a given radius
    /// </summary>
    [Serializable]
    [QueryEditor("Location/In Radius", "In radius [m_Radius] from object [m_Source]")]
    public struct DistanceFilter : IQueryFilter
    {
        /// <summary>
        /// Radius within an object is considered valid
        /// </summary>
        public float Radius
        {
            get => m_Radius;
            set => m_Radius = value;
        }

        /// <summary>
        /// Source object used to calculate the distance
        /// </summary>
        public SemanticObject Source
        {
            get => m_Source;
            set => m_Source = value;
        }

#pragma warning disable 0649
        [SerializeField]
        float m_Radius;

        [SerializeField]
        SemanticObject m_Source;
#pragma warning restore 0649

        /// <inheritdoc />
        public void Validate(EntityManager entityManager, NativeArray<Entity> entities, UnsafeBitArray entitiesValid)
        {
            // TODO: Force to have location trait?
            // Using GetComponentObject on every entities should be avoided

            Vector3 sourcePosition = Vector3.zero;
            if (m_Source != null)
            {
                var sourceTransform = m_Source.GetComponent<Transform>();
                sourcePosition = sourceTransform.position;
            }

            for (var i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                // Only check entities that haven't already failed queries
                if (entitiesValid.IsSet(i))
                {
                    var entityTransform = entityManager.GetComponentObject<Transform>(entity);
                    if (entityTransform
                        && Vector3.Distance(sourcePosition, entityTransform.position) > m_Radius)
                        entitiesValid.Set(i, false);
                }
            }
        }
    }
}
