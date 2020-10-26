#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Semantic.Traits;
using Unity.Semantic.Traits.Utility;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace UnityEditor.Semantic.Traits.Utility
{
    [InitializeOnLoad]
    static class TraitAssetDatabase
    {
        static IEnumerable<TraitDefinition> s_TraitDefinitions;
        static IEnumerable<EnumDefinition> s_EnumDefinitions;

        static Dictionary<TraitDefinition, Type> s_TraitDefinitionToType = new Dictionary<TraitDefinition, Type>();
        static Dictionary<Type, TraitDefinition> s_TypeToTraitDefinition = new Dictionary<Type, TraitDefinition>();

        static TraitAssetDatabase()
        {
            Refresh();
        }

        public static IEnumerable<TraitDefinition> TraitDefinitions
        {
            get
            {
                if (s_TraitDefinitions == null)
                {
                    UpdateTraitDefinitions();
                }

                return s_TraitDefinitions;
            }
        }

        public static IEnumerable<EnumDefinition> EnumDefinitions
        {
            get
            {
                if (s_EnumDefinitions == null)
                {
                    UpdateEnumDefinitions();
                }

                return s_EnumDefinitions;
            }
        }

        public static void Refresh()
        {
            UpdateEnumDefinitions();
            UpdateTraitDefinitions();
        }

        public static TraitDefinition GetTraitDefinitionForType(Type type)
        {
            return s_TypeToTraitDefinition.TryGetValue(type, out var traitDefinition) ? traitDefinition : null;
        }

        public static Type GetTypeFromTraitDefinition(TraitDefinition traitDefinition)
        {
            return s_TraitDefinitionToType.TryGetValue(traitDefinition, out var type) ? type : null;
        }

        static void UpdateEnumDefinitions()
        {
            s_EnumDefinitions = AssetDatabase.FindAssets($"t: {nameof(EnumDefinition)}").Select(guid =>
                AssetDatabase.LoadAssetAtPath<EnumDefinition>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(d => d != null);;
        }

        static void UpdateTraitDefinitions()
        {
            s_TraitDefinitions = AssetDatabase.FindAssets($"t: {nameof(TraitDefinition)}").Select(guid =>
                AssetDatabase.LoadAssetAtPath<TraitDefinition>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(d => d != null);

            s_TraitDefinitionToType.Clear();
            s_TypeToTraitDefinition.Clear();

            var traitTypes = TypeCache.GetTypesDerivedFrom<ITrait>();
            var traitDataTypes = TypeCache.GetTypesDerivedFrom<ITraitData>();

            foreach (var definition in s_TraitDefinitions)
            {
                var type = traitTypes.FirstOrDefault(t => t.Name == definition.name);

                // Type if not generated yet
                if (type == null)
                    continue;

                s_TraitDefinitionToType[definition] = type;
                s_TypeToTraitDefinition[type] = definition;

                var componentType = traitDataTypes.FirstOrDefault(t => t.Name == definition.name + TypeResolver.ComponentDataSuffix);
                if (componentType == null)
                    continue;

                s_TypeToTraitDefinition[componentType] = definition;
            }
        }
    }
}
#endif
