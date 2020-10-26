using Unity.Semantic.Traits.Utility;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Unity.Semantic.Traits.Queries
{
    static class AssemblyRegistration
    {
        internal const string TraitQueriesQualifier = "Unity.Semantic.Traits.Queries";

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#endif
        static void RegisterResolver()
        {
            TypeResolver.AddResolver($"{{0}}, {TraitQueriesQualifier}");
        }
    }
}
