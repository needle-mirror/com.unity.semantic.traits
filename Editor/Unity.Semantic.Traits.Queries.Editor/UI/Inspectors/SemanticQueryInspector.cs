using System.Collections.Generic;
using Unity.Semantic.Traits;
using Unity.Semantic.Traits.Queries;
using Unity.Collections;
using Unity.Entities;
using UnityEditor.Semantic.Traits.Utility;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Semantic.Traits.Queries.UI
{
    [CustomEditor(typeof(SemanticQuery), true)]
    class SemanticQueryInspector : Editor
    {
        const string k_PreviewResultContainerName = "previewResults";

        VisualElement m_InspectorRoot;
        VisualElement m_PreviewResult;

        void OnEnable()
        {
            TraitAssetDatabase.Refresh();
            EditorApplication.update += RefreshMode;
        }

        void OnDisable()
        {
            EditorApplication.update -= RefreshMode;
        }

        void RefreshMode()
        {
            if (m_PreviewResult == null || target == null)
                return;

            m_PreviewResult.Clear();

            var world = World.DefaultGameObjectInjectionWorld;
            var validTraitBasedObjects = new List<SemanticObject>();
            if (world != null && world.IsCreated)
            {
                var entityManager = world.EntityManager;
                var queryProperty = serializedObject.FindProperty("m_Query");
                var objectQuery = SerializedPropertyExtensions.GetValue<TraitBasedObjectQuery>(queryProperty);

                var entityQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<SemanticObjectData>());
                using (var entities = entityQuery.ToEntityArray(Allocator.TempJob))
                {
                    objectQuery.Validate(entityManager, entities, (_, entity) =>
                    {
                        var transform = entityManager.GetComponentObject<Transform>(entity);
                        if (transform)
                        {
                            var traitBasedObject = transform.GetComponent<SemanticObject>();
                            validTraitBasedObjects.Add(traitBasedObject);
                        }
                    });
                }
            }

            if (validTraitBasedObjects.Count > 0)
            {
                foreach (var semanticObject in validTraitBasedObjects)
                {
                    if (semanticObject.gameObject == null)
                        continue;

                    var o = new ObjectField();
                    o.SetValueWithoutNotify(semanticObject);
                    m_PreviewResult.Add(o);
                }
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            m_InspectorRoot = new VisualElement();
            m_InspectorRoot.Add(new SemanticQueryView(serializedObject, "m_Query"));

            // Add preview result
            m_PreviewResult = m_InspectorRoot.Q<VisualElement>(k_PreviewResultContainerName);

            return m_InspectorRoot;
        }
    }
}
