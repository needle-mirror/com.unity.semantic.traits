using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Semantic.Traits.UI
{
    [CustomPropertyDrawer(typeof(InspectorNameAttribute))]
    class InspectorNamePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var inspectorNameAttribute = (InspectorNameAttribute)attribute;
            EditorGUI.PropertyField(position, property, new GUIContent(inspectorNameAttribute.displayName));
        }
    }
}
