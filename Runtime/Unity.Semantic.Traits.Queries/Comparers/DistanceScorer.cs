using System;
using Unity.Semantic.Traits;
using Unity.Semantic.Traits.Queries;
using Unity.Entities;
using UnityEngine;

namespace Unity.Semantic.Traits.Queries
{
    /// <summary>
    /// A scorer depending on the distance with a reference object
    /// </summary>
    [Serializable]
    [QueryEditor("Location/Distance from object", "Nearest from [m_Reference] object")]
    public struct DistanceScorer : IQueryScorer
    {
#pragma warning disable 0649
        [SerializeField]
        SemanticObject m_Reference;
#pragma warning restore 0649

        /// <inheritdoc />
        public float GetScore(EntityManager entityManager, Entity entity)
        {
            if (m_Reference == null)
                return 0;

            // TODO : Value should be read from Location component and set has a required Trait for this scorer
            var referencePosition = m_Reference.GetComponent<Transform>().position;
            var entityPosition = entityManager.GetComponentObject<Transform>(entity);

            return Vector3.Distance(entityPosition.position, (referencePosition));
        }
    }
}
