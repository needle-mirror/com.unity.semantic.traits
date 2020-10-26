using System;
using System.Collections.Generic;
using Unity.Semantic.Traits;
using Unity.Semantic.Traits.Queries;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Unity.Semantic.Traits.Queries
{
    /// <summary>
    /// A component to create queries to retrieve Semantic objects from the world
    /// </summary>
    [AddComponentMenu(Constants.MenuName + "/Semantic Query")]
    public class SemanticQuery : MonoBehaviour
    {
        internal IList<QueryConditionalGroup> Groups => m_Query.m_QueryGroups;

#pragma warning disable 0649
        [SerializeField]
        TraitBasedObjectQuery m_Query;
#pragma warning restore 0649

        // Used locally for caching purposes
        List<SemanticObject> m_SemanticObjectCache = new List<SemanticObject>();

        void Awake()
        {
            if (m_Query.m_QueryGroups == null)
                m_Query.m_QueryGroups = new List<QueryConditionalGroup>();

            foreach (var group in m_Query.m_QueryGroups)
            {
                group.Initialize();
            }
        }

        /// <summary>
        /// Get back a collection of semantic objects based on the query
        /// </summary>
        /// <returns>A collection of semantic objects</returns>
        public IEnumerable<SemanticObject> GetSemanticObjects()
        {
            m_SemanticObjectCache.Clear();

            var world = World.DefaultGameObjectInjectionWorld;
            if (world != null && world.IsCreated)
            {
                var entityManager = world.EntityManager;
                var entityQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<SemanticObjectData>());
                using (var entities = entityQuery.ToEntityArray(Allocator.TempJob))
                {
                    m_Query.Validate(entityManager, entities, OnValidEntity);
                }
            }

            return m_SemanticObjectCache;
        }

        void OnValidEntity(EntityManager entityManager, Entity entity)
        {
            var entityTransform = entityManager.GetComponentObject<Transform>(entity);
            if (entityTransform)
            {
                var traitBasedObject = entityTransform.GetComponent<SemanticObject>();
                m_SemanticObjectCache.Add(traitBasedObject);
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_Query.m_QueryGroups == null)
                m_Query.m_QueryGroups = new List<QueryConditionalGroup>();

            foreach (var group in Groups)
            {
                // Check filter lists for Trait requirements that needs to be added to the group
                foreach (var filter in group.m_Filters)
                {
                    var filterType = filter.GetType();
                    if (filterType.IsDefined(typeof(QueryEditorAttribute), true))
                    {
                        var queryFormat = (QueryEditorAttribute)filterType.GetCustomAttributes(typeof(QueryEditorAttribute), true)[0];
                        if (queryFormat.RequiredTraitData != null)
                        {
                            var traitDefinition = UnityEditor.Semantic.Traits.Utility.TraitAssetDatabase.GetTraitDefinitionForType(queryFormat.RequiredTraitData);
                            group.AddRequiredTrait(traitDefinition);
                        }
                    }
                }

                // Check scorer method for Trait requirements that needs to be added to the group
                if (group.m_QuantifierScorer != null)
                {
                    var comparerType = group.m_QuantifierScorer.GetType();
                    if (comparerType.IsDefined(typeof(QueryEditorAttribute), true))
                    {
                        var queryFormat = (QueryEditorAttribute)comparerType.GetCustomAttributes(typeof(QueryEditorAttribute), true)[0];
                        if (queryFormat.RequiredTraitData != null)
                        {
                            var traitDefinition = UnityEditor.Semantic.Traits.Utility.TraitAssetDatabase.GetTraitDefinitionForType(queryFormat.RequiredTraitData);
                            group.AddRequiredTrait(traitDefinition);
                        }
                    }
                }
            }
        }
#endif
    }
}
