using System.Collections.Generic;
using Unity.Entities;

namespace Unity.Semantic.Traits.Queries
{
    /// <summary>
    /// Interface used to declare a scorer that can be used to sort Semantic Query results
    /// </summary>
    public interface IQueryScorer
    {
        /// <summary>
        /// Give a score for an entity based on its trait ComponentData
        /// </summary>
        /// <param name="entityManager">Entity Manager</param>
        /// <param name="entity">Entity to be evaluated</param>
        /// <returns>Score value</returns>
        float GetScore(EntityManager entityManager, Entity entity);
    }
}
