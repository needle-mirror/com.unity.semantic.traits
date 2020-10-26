using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Semantic.Traits.Utility;
using UnityEngine;

namespace Unity.Semantic.Traits.Queries
{
    [Serializable]
    struct QueryConditionalGroup
    {
        internal enum ConditionalQuantifiers
        {
            All = 0,
            First,
            Last,
        }

        [SerializeReference]
        internal List<IQueryFilter> m_Filters;

        [SerializeField]
        internal ConditionalQuantifiers m_Quantifier;

        [SerializeField]
        internal int m_QuantifierLimit;

        [SerializeReference]
        internal IQueryScorer m_QuantifierScorer;

        // Used for serialization to generate a 'WithTraitFilter' at runtime
        [SerializeField]
        [TraitSelector(TraitSelectorAttribute.TraitFilter.Required), InspectorName("With Traits")]
        TraitCollection m_RequiredTraits;

        // Used for serialization to generate a 'WithoutTraitFilter' at runtime
        [SerializeField]
        [TraitSelector(TraitSelectorAttribute.TraitFilter.Prohibited), InspectorName("Without Traits")]
        TraitCollection m_ProhibitedTraits;

        internal void Initialize()
        {
            if (m_RequiredTraits.Count > 0)
                m_Filters.Insert(0, new WithTraitTypesFilter { TraitTypes = ConvertTraitCollectionToTypes(m_RequiredTraits) });

            if (m_ProhibitedTraits.Count > 0)
                m_Filters.Insert(0, new WithoutTraitTypesFilter { TraitTypes = ConvertTraitCollectionToTypes(m_ProhibitedTraits) });

#if !UNITY_EDITOR
            // Release references to Trait definition assets
            m_RequiredTraits.Clear();
            m_ProhibitedTraits.Clear();
#endif
        }

        static ComponentType[] ConvertTraitCollectionToTypes(TraitCollection collection)
        {
            var traitTypes = new ComponentType[collection.Count];
            for (var i = 0; i < collection.Count; i++)
            {
                var traitDefinition = collection[i];
                if (TypeResolver.TryGetType($"{traitDefinition.name}{TypeResolver.ComponentDataSuffix}", out var traitType))
                    traitTypes[i] = traitType;
            }

            return traitTypes;
        }

        public void Validate(EntityManager entityManager, NativeArray<Entity> entities, UnsafeBitArray entitiesValid)
        {
#if UNITY_EDITOR
            // Create temporary filters in editor mode to display query preview
            if (!Application.isPlaying)
            {
                if (m_RequiredTraits.Count > 0)
                {
                    var filter = new WithTraitTypesFilter { TraitTypes = ConvertTraitCollectionToTypes(m_RequiredTraits) };
                    filter.Validate(entityManager, entities, entitiesValid);
                }

                if (m_ProhibitedTraits.Count > 0)
                {
                    var filter = new WithoutTraitTypesFilter { TraitTypes = ConvertTraitCollectionToTypes(m_ProhibitedTraits) };
                    filter.Validate(entityManager, entities, entitiesValid);
                }
            }
#endif

            foreach (var filter in m_Filters)
            {
                filter.Validate(entityManager, entities, entitiesValid);
            }

            if (m_Quantifier != ConditionalQuantifiers.All && m_QuantifierLimit > 0)
            {
                if (m_QuantifierScorer != default)
                {
                    using(var heap = new FixedMinHeap<int>(m_QuantifierLimit, Allocator.TempJob))
                    {
                        for (var i = 0; i < entities.Length; i++)
                        {
                            if (entitiesValid.IsSet(i))
                            {
                                var entity = entities[i];
                                var entityScore = m_QuantifierScorer.GetScore(entityManager, entity);

                                if (m_Quantifier == ConditionalQuantifiers.Last)
                                    entityScore = -entityScore;

                                heap.TryInsert(entityScore, i);
                            }
                        }

                        // Un-validate every entities except the ones that are present in best indexes list
                        entitiesValid.SetBits(0, false, entities.Length);
                        for (var i = 0; i < heap.Data.Length; i++)
                        {
                            var index = heap.Data[i];
                            entitiesValid.Set(index, true);
                        }
                    }
                }
                else
                {
                    var validCount = 0;
                    for (var i = 0; i < entities.Length; i++)
                    {
                        if (entitiesValid.IsSet(i))
                        {
                            if (validCount < m_QuantifierLimit)
                                validCount++;
                            else
                                entitiesValid.Set(i, false);
                        }
                    }
                }
            }
        }

        internal void AddRequiredTrait(TraitDefinition traitDefinition)
        {
            m_RequiredTraits.Add(traitDefinition);
        }
    }

    [Serializable]
    struct TraitBasedObjectQuery
    {
#pragma warning disable 0649
        [SerializeField]
        internal List<QueryConditionalGroup> m_QueryGroups;
#pragma warning restore 0649

        public TraitBasedObjectQuery Clone()
        {
            var clone = this;
            clone.m_QueryGroups = new List<QueryConditionalGroup>(m_QueryGroups);
            return clone;
        }

        public void Validate(EntityManager entityManager, NativeArray<Entity> entities,
            Action<EntityManager, Entity> validCallback)
        {
            if (entities.Length == 0)
                return;

            if ((m_QueryGroups == null || m_QueryGroups.Count == 0))
            {
                foreach (var entity in entities)
                {
                    validCallback(entityManager, entity);
                }
                return;
            }

            var previousEntitiesValid = default(UnsafeBitArray);
            var entitiesValid = new UnsafeBitArray(entities.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);


            foreach (var queryGroup in m_QueryGroups)
            {
                entitiesValid.SetBits(0, true, entities.Length);

                queryGroup.Validate(entityManager, entities, entitiesValid);

                unsafe
                {
                    if (previousEntitiesValid.IsCreated)
                    {
                        // Combine new results with previous results
                        OrBits(previousEntitiesValid, entitiesValid);
                    }
                    else
                    {
                        // Cache previous entries
                        previousEntitiesValid = new UnsafeBitArray(entitiesValid.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                        UnsafeUtility.MemCpy(previousEntitiesValid.Ptr, entitiesValid.Ptr, entitiesValid.Length / 8);
                    }
                }
            }

            // If an OR has been used, then make sure to combine new results with previous
            if (previousEntitiesValid.IsCreated)
            {
                // Combine previous results with new results
                OrBits(entitiesValid, previousEntitiesValid);
                previousEntitiesValid.Dispose();
            }

            for (var i = 0; i < entities.Length; i++)
            {
                if (entitiesValid.IsSet(i))
                {
                    var entity = entities[i];
                    validCallback(entityManager, entity);
                }
            }

            entitiesValid.Dispose();
        }

        static void OrBits(UnsafeBitArray dst, UnsafeBitArray src)
        {
            if (!dst.IsCreated || !src.IsCreated)
                throw new ArgumentException("Both bit arrays must be created");

            if (dst.Length != src.Length)
                throw new ArgumentException("Both bit arrays must be of the same length");

            var i = 0;
            while (i < src.Length)
            {
                const int stride = sizeof(ulong) * 8;
                var previousBits = dst.GetBits(i, stride);
                var newBits = src.GetBits(i, stride);
                dst.SetBits(i, previousBits | newBits, stride);
                i += stride;
            }
        }

        public QueryConditionalGroup GetLastGroupOrCreate()
        {
            if (m_QueryGroups == null)
                m_QueryGroups = new List<QueryConditionalGroup>();

            if (m_QueryGroups.Count == 0)
               AddConditionalGroup();

            return m_QueryGroups[m_QueryGroups.Count - 1];
        }

        internal void AddConditionalGroup()
        {
            m_QueryGroups.Add(new QueryConditionalGroup { m_Filters = new List<IQueryFilter>() });
        }
    }
}
