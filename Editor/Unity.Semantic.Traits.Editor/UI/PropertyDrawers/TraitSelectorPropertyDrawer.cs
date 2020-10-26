using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Semantic.Traits;
using UnityEditor.Semantic.Traits.Utility;
using UnityEngine;

namespace UnityEditor.Semantic.Traits.UI
{
    [CustomPropertyDrawer(typeof(TraitSelectorAttribute), true)]
    class TraitSelectorPropertyDrawer : PropertyDrawer
    {
        Func<TraitDefinition, bool> m_DisplayTrait;

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var inspectorNameAttribute = fieldInfo.GetCustomAttributes<InspectorNameAttribute>().FirstOrDefault();
            if (inspectorNameAttribute != null)
                label.text = inspectorNameAttribute.displayName;
            EditorGUI.LabelField(rect, label);
            var selectorAttribute = (TraitSelectorAttribute)attribute;
            var requiredTraits = selectorAttribute.Filter == TraitSelectorAttribute.TraitFilter.Required;

            var displayTraitType = selectorAttribute.DisplayTraitType;
            if (displayTraitType != null)
            {
                var method = displayTraitType.GetMethod("DisplayTrait", BindingFlags.Public | BindingFlags.Static);
                m_DisplayTrait = (td) => (bool)method.Invoke(null, new object[] { td });
            }

            var labelLength = EditorStyles.label.CalcSize(label).x + 5;
            rect.x += labelLength;
            rect.width -= labelLength;
            rect.y += 2;

            var traitProperty = property.FindPropertyRelative("m_Traits");
            if (traitProperty == null)
                traitProperty = property;

            DrawSelector(traitProperty, rect, selectorAttribute.Filter.ToString(),
                requiredTraits ? EditorStyleHelper.requiredTraitLabel : EditorStyleHelper.prohibitedTraitLabel,
                requiredTraits ? EditorStyleHelper.requiredTraitAdd : EditorStyleHelper.prohibitedTraitAdd,
                requiredTraits ? EditorStyleHelper.requiredTraitMore : EditorStyleHelper.prohibitedTraitMore,
                displayTrait: m_DisplayTrait);
        }

        public static void DrawSelector(SerializedProperty traits, Rect rect, string title, GUIStyle style,
            GUIStyle buttonStyle, GUIStyle altButtonStyle, IEnumerable<TraitDefinition> invalidTraits = null,
            Func<TraitDefinition, bool> displayTrait = null)
        {
            Rect labelRect = rect;
            labelRect.height = style.fixedHeight + 1;

            var allTraitsDisplayed = true;
            var showAddButton = true;
            if (traits.isArray)
            {
                traits.ForEachArrayElement(e =>
                    ShowTrait(traits, e, rect, ref labelRect, title, style, altButtonStyle, ref allTraitsDisplayed,
                        invalidTraits, displayTrait));
            }
            else
            {
                ShowTrait(traits, traits, rect, ref labelRect, title, style, altButtonStyle, ref allTraitsDisplayed,
                    invalidTraits, displayTrait);

                showAddButton = traits.objectReferenceValue == null;
            }

            if (showAddButton)
            {
                var addButtonStyle = allTraitsDisplayed ? buttonStyle : altButtonStyle;
                labelRect.width = addButtonStyle.normal.background.width;
                if (GUI.Button(labelRect, string.Empty, addButtonStyle))
                {
                    PopupWindow.Show(labelRect, new TraitSelectorPopup(title, traits, invalidTraits, displayTrait));
                }
            }
        }

        static void ShowTrait(SerializedProperty traits, SerializedProperty trait, Rect rect, ref Rect labelRect,
            string title, GUIStyle style, GUIStyle altButtonStyle, ref bool allTraitsDisplayed,
            IEnumerable<TraitDefinition> invalidTraits = null, Func<TraitDefinition, bool> displayTrait = null)
        {
            var asset = trait.objectReferenceValue as TraitDefinition;

            if (asset == null)
                return;

            var size = style.CalcSize(new GUIContent(asset.name));
            labelRect.width = size.x;

            if (labelRect.xMax + altButtonStyle.normal.background.width > rect.xMax)
            {
                allTraitsDisplayed = false;
                return;
            }

            if (style == EditorStyleHelper.requiredTraitLabel)
                GUI.backgroundColor = asset.Color;

            if (GUI.Button(labelRect, asset.name, style))
            {
                PopupWindow.Show(labelRect, new TraitSelectorPopup(title, traits, invalidTraits, displayTrait));
            }
            GUI.backgroundColor = Color.white;

            labelRect.x += size.x + 2;
        }
    }
}
