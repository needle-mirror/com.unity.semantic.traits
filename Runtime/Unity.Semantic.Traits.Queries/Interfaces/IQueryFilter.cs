using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

namespace Unity.Semantic.Traits.Queries
{
    /// <summary>
    /// Interface used to declare a filter that can be used in a Semantic Query
    /// </summary>
    public interface IQueryFilter
    {
        /// <summary>
        /// Check a list of entity validity and marks non-valid ones inside entitiesValid
        /// </summary>
        /// <param name="entityManager">Entity manager</param>
        /// <param name="entities">List of entity</param>
        /// <param name="entitiesValid">Array of bit that marks if an entity is valid for the current query</param>
        void Validate(EntityManager entityManager, NativeArray<Entity> entities, UnsafeBitArray entitiesValid);
    }
}
