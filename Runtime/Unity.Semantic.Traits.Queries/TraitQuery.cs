using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace Unity.Semantic.Traits.Queries
{
    /// <summary>
    /// Query object
    /// </summary>
    public struct TraitQuery : IEnumerable<Entity>, IDisposable
    {
        /// <summary>
        /// Access the element at the specified index from the trait query results
        /// </summary>
        /// <param name="index">Index of the element from the results</param>
        public Entity this[int index] => m_QueryResults.IsCreated ? QueryResults[index] : default;

        /// <summary>
        /// Count of entities resulting from the trait query
        /// </summary>
        public int EntityCount => m_QueryResults.IsCreated ? QueryResults.Length : -1;

        NativeList<Entity> QueryResults
        {
            get
            {
                CheckValidOrThrow();

                if (m_QueryCompleted)
                    return m_QueryResults;

                PerformQuery();
                return m_QueryResults;
            }
        }

        internal TraitBasedObjectQuery m_Query;
        EntityManager m_EntityManager;
        NativeList<Entity> m_QueryResults;
        bool m_QueryCompleted;
        int m_Version;
        NativeArray<int> m_ValidVersion;

        /// <summary>
        /// Create a new TraitQuery, which caches results, and must be disposed after its use
        /// </summary>
        /// <param name="entityManager">The entity manager that will be used for entity queries</param>
        public TraitQuery(EntityManager entityManager)
        {
            m_EntityManager = entityManager;
            m_Query = new TraitBasedObjectQuery();
            m_QueryResults = new NativeList<Entity>(entityManager.EntityCapacity, Allocator.TempJob);
            m_QueryCompleted = false;
            m_Version = 0;
            m_ValidVersion = new NativeArray<int>(1, Allocator.TempJob);
        }

        /// <summary>
        /// Clone an existing trait query (in order to preserve the original)
        /// </summary>
        /// <returns>A copy of this trait query with a new cache for query results (that must be disposed)</returns>
        public TraitQuery Clone()
        {
            CheckValidOrThrow();

            var clone = this;
            clone.m_Query = m_Query.Clone();
            clone.m_QueryResults = new NativeList<Entity>(m_QueryResults.Capacity, Allocator.TempJob);
            clone.m_Version = 0;
            clone.m_ValidVersion = new NativeArray<int>(1, Allocator.TempJob);
            return clone;
        }

        /// <summary>
        /// Constrain the trait query further with a filter (use <see cref="Clone"/> to preserve original)
        /// </summary>
        /// <param name="filter">Filter to add to the trait query</param>
        /// <typeparam name="T">A filter type that implements IQueryFilter</typeparam>
        /// <returns>An updated trait query w/ the filter</returns>
        public TraitQuery WithFilter<T>(T filter) where T : IQueryFilter
        {
            CheckValidOrThrow();

            var updated = this;
            updated.m_Query.GetLastGroupOrCreate().m_Filters.Add(filter);
            updated.m_Version++;

            // Invalidate this copy, since the filters / results cache are shared
            m_ValidVersion[0] = updated.m_Version;

            return updated;
        }

        /// <summary>
        /// Add a new conditional group to the query with an OR operator
        /// </summary>
        /// <returns>An updated trait query with a new conditional group</returns>
        public TraitQuery Or()
        {
            CheckValidOrThrow();

            var updated = this;
            updated.m_Query.AddConditionalGroup();
            updated.m_Version++;

            // Invalidate this copy, since the filters / results cache are shared
            m_ValidVersion[0] = updated.m_Version;

            return updated;
        }

        /// <summary>
        /// Reset the current cached results of the query, so that it can be re-executed
        /// </summary>
        public void Reset()
        {
            CheckValidOrThrow();

            m_QueryCompleted = false;
            m_QueryResults.Clear();
        }

        /// <summary>
        /// Return the query result or perform the query if not completed
        /// </summary>
        /// <returns>Query result</returns>
        public IEnumerator<Entity> GetEnumerator()
        {
            CheckValidOrThrow();

            if (m_QueryCompleted)
                return m_QueryResults.GetEnumerator();

            PerformQuery();
            return m_QueryResults.GetEnumerator();
        }

        void CheckValidOrThrow()
        {
            if (m_ValidVersion.IsCreated && m_ValidVersion[0] == m_Version)
                return;

            if (m_EntityManager == default || !m_EntityManager.World.IsCreated)
                throw new ArgumentException(
                    "TraitQuery doesn't have a valid EntityManager; Did you use the default constructor by mistake?");

            throw new InvalidOperationException(
                "TraitQuery has already been invalidated by an existing method chaining (e.g. WithFilter) invocation.");
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void PerformQuery()
        {
            var entityManager = m_EntityManager;
            var world = m_EntityManager.World;
            if (world != null && world.IsCreated)
            {
                var entityQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<SemanticObjectData>());
                using (var entities = entityQuery.ToEntityArray(Allocator.TempJob))
                {
                    m_Query.Validate(entityManager, entities, AddToResults);
                }
            }

            m_QueryCompleted = true;
        }

        void AddToResults(EntityManager manager, Entity validEntity)
        {
            if (m_QueryResults.IsCreated)
                m_QueryResults.Add(validEntity);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            CheckValidOrThrow();

            if (m_QueryResults.IsCreated)
                m_QueryResults.Dispose();

            if (m_ValidVersion.IsCreated)
                m_ValidVersion.Dispose();
        }
    }
}
