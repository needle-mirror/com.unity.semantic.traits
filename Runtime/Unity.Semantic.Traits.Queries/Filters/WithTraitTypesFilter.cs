using System;
using Unity.Semantic.Traits.Utility;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

namespace Unity.Semantic.Traits.Queries
{
    /// <summary>
    /// Filter that checks if a semantic object contains a specified list of Traits
    /// </summary>
    [Serializable]
    struct WithTraitTypesFilter : IQueryFilter
    {
        public ComponentType[] TraitTypes;

        /// <inheritdoc />
        public void Validate(EntityManager entityManager, NativeArray<Entity> entities, UnsafeBitArray entitiesValid)
        {
            var entityQuery = entityManager.CreateEntityQuery(TraitTypes);
            var queryMask = entityManager.GetEntityQueryMask(entityQuery);
            for (var i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                // Only check entities that haven't already failed queries
                if (entitiesValid.IsSet(i) && !queryMask.Matches(entity))
                    entitiesValid.Set(i, false);
            }
        }
    }

    /// <summary>
    /// Helper extensions to instantiate WithTraitTypesFilter
    /// </summary>
    public static class WithTraitTypesExtension
    {
        /// <summary>
        /// Extension method to create a WithTraitTypesFilter with one Trait type
        /// </summary>
        /// <param name="traitQuery">Query object</param>
        /// <typeparam name="T">Trait type</typeparam>
        /// <returns>Updated query object with the filter added</returns>
        public static TraitQuery WithTraitTypes<T>(this TraitQuery traitQuery) where T : ITraitData
        {
            return traitQuery.WithFilter(new WithTraitTypesFilter
            {
                TraitTypes = new ComponentType[] { typeof(T) }
            });
        }

        /// <summary>
        /// Extension method to create a WithTraitTypesFilter with two Trait types
        /// </summary>
        /// <param name="traitQuery">Query object</param>
        /// <typeparam name="T1">Trait type</typeparam>
        /// <typeparam name="T2">Trait type</typeparam>
        /// <returns>Updated query object with the filter added</returns>
        public static TraitQuery WithTraitTypes<T1,T2>(this TraitQuery traitQuery)
            where T1 : ITraitData
            where T2 : ITraitData
        {
            return traitQuery.WithFilter(new WithTraitTypesFilter
            {
                TraitTypes = new ComponentType[] { typeof(T1), typeof(T2) }
            });
        }

        /// <summary>
        /// Extension method to create a WithTraitTypesFilter with three Trait types
        /// </summary>
        /// <param name="traitQuery">Query object</param>
        /// <typeparam name="T1">Trait type</typeparam>
        /// <typeparam name="T2">Trait type</typeparam>
        /// <typeparam name="T3">Trait type</typeparam>
        /// <returns>Updated query object with the filter added</returns>
        public static TraitQuery WithTraitTypes<T1,T2,T3>(this TraitQuery traitQuery)
            where T1 : ITraitData
            where T2 : ITraitData
            where T3 : ITraitData
        {
            return traitQuery.WithFilter(new WithTraitTypesFilter
            {
                TraitTypes = new ComponentType[] { typeof(T1), typeof(T2), typeof(T3) }
            });
        }

        /// <summary>
        /// Extension method to create a WithTraitTypesFilter with an array of Trait types
        /// </summary>
        /// <param name="traitQuery">Query object</param>
        /// <param name="traitTypes">Array of Trait types</param>
        /// <returns>Updated query object with the filter added</returns>
        public static TraitQuery WithTraitTypes(this TraitQuery traitQuery, params ComponentType[] traitTypes)
        {
            return traitQuery.WithFilter(new WithTraitTypesFilter
            {
                TraitTypes = traitTypes
            });
        }
    }
}
