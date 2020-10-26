using System;
using System.Collections.Generic;
using Unity.Semantic.Traits.Utility;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

namespace Unity.Semantic.Traits.Queries
{
    /// <summary>
    /// Filter that holds a delegate to a custom validity function
    /// </summary>
    /// <typeparam name="T">Trait type</typeparam>
    [Serializable]
    public struct ConditionFilter<T> : IQueryFilter where T : struct, ITraitData
    {
        /// <summary>
        /// Delegate used for validating an object
        /// </summary>
        /// <param name="other">Trait value</param>
        /// <param name="reference">Reference trait data</param>
        public delegate bool ValidComparer(ref T other, ref T reference);

        /// <summary>
        /// Object used as a reference in the validity check
        /// </summary>
        public T ReferenceData;

        /// <summary>
        /// Delegate validity function
        /// </summary>
        public ValidComparer Valid;

        /// <inheritdoc />
        public void Validate(EntityManager entityManager, NativeArray<Entity> entities, UnsafeBitArray entitiesValid)
        {
            for (var i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                if (!entityManager.HasComponent<T>(entity))
                    continue;

                var traitData = entityManager.GetComponentData<T>(entity);

                // Only check entities that haven't already failed queries
                if (entitiesValid.IsSet(i) && !Valid(ref traitData, ref ReferenceData))
                    entitiesValid.Set(i, false);
            }
        }
    }

    /// <summary>
    /// Helper extensions to instantiate ConditionFilter
    /// </summary>
    public static class ConditionTypesExtension
    {
        /// <summary>
        /// Extension method to create a ConditionFilter with reference Data
        /// </summary>
        /// <param name="traitQuery">Query object</param>
        /// <param name="referenceData">Data used as reference</param>
        /// <param name="comparer">Delegate function</param>
        /// <typeparam name="T">Trait type</typeparam>
        /// <returns>Updated query object with the filter added</returns>
        public static TraitQuery Where<T>(this TraitQuery traitQuery, T referenceData, ConditionFilter<T>.ValidComparer comparer) where T : struct, ITraitData
        {
            return traitQuery.WithTraitTypes<T>().WithFilter(new ConditionFilter<T>
            {
                ReferenceData = referenceData,
                Valid = comparer
            });
        }

        /// <summary>
        /// Extension method to create a ConditionFilter
        /// </summary>
        /// <param name="traitQuery">Query object</param>
        /// <param name="comparer">Delegate function</param>
        /// <typeparam name="T">Trait type</typeparam>
        /// <returns>Updated query object with the filter added</returns>
        public static TraitQuery Where<T>(this TraitQuery traitQuery, ConditionFilter<T>.ValidComparer comparer) where T : struct, ITraitData
        {
            // This extension would likely be used for closures, which will allocate memory, unfortunately
            return traitQuery.WithTraitTypes<T>().WithFilter(new ConditionFilter<T>
            {
                Valid = comparer
            });
        }
    }
}
