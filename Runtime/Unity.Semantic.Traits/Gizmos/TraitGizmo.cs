#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Unity.Semantic.Traits
{
    class TraitGizmoMethod
    {
        internal Action<GameObject, ITraitData, bool> drawMethod;

        public void Invoke(GameObject gameObject, ITraitData traitData, bool isSelected)
        {
            drawMethod?.Invoke(gameObject, traitData, isSelected);
        }
    }

    static class TraitGizmos
    {
        const string k_GizmoMethodName = "DrawGizmos";

        static Dictionary<string, TraitGizmoMethod> s_DrawGizmoMethods = new Dictionary<string, TraitGizmoMethod>();

        internal static void DrawGizmoForTrait(string traitName, GameObject gameObject, ITraitData data)
        {
            bool isSelected = Selection.activeGameObject == gameObject;
            var method = GetGizmoMethod(traitName);
            method?.Invoke(gameObject, data, isSelected);

            // TODO provide a default gizmo when no custom method exists
        }

        static TraitGizmoMethod GetGizmoMethod(string traitName)
        {
            if (s_DrawGizmoMethods.ContainsKey(traitName))
            {
                return s_DrawGizmoMethods[traitName];
            }

            var types = TypeCache.GetTypesWithAttribute<TraitGizmoAttribute>();
            foreach (var type in types)
            {
                var attributes = type.GetCustomAttributes(typeof(TraitGizmoAttribute), false);
                foreach (TraitGizmoAttribute gizmo in attributes)
                {
                    if (gizmo.m_TraitType.Name == traitName)
                    {
                        MethodInfo info = type.GetMethod(k_GizmoMethodName);
                        if (info == null)
                        {
                            Debug.LogError($"Fail to find {k_GizmoMethodName} method in type {type.Name}.");
                            continue;
                        }

                        var gizmoInstance = Activator.CreateInstance(type);
                        var delegateMethod = (Action<GameObject, ITraitData, bool>)Delegate.CreateDelegate(typeof(Action<GameObject, ITraitData, bool>), gizmoInstance, info);

                        var gizmoMethod = new TraitGizmoMethod { drawMethod = delegateMethod };

                        s_DrawGizmoMethods.Add(traitName, new TraitGizmoMethod { drawMethod = delegateMethod });
                        return gizmoMethod;
                    }
                }
            }

            s_DrawGizmoMethods.Add(traitName, null);
            return null;
        }
    }
}
#endif
