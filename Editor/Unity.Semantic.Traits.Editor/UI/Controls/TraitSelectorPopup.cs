using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Semantic.Traits;
using UnityEditor.Semantic.Traits.Utility;
using UnityEngine;

namespace UnityEditor.Semantic.Traits.UI
{
    class TraitSelectorPopup : PopupWindowContent
    {
        SerializedProperty m_Property;
        IEnumerable<TraitDefinition> m_InvalidTraits;
        List<TraitDefinition> m_TraitsSelected = new List<TraitDefinition>();
        string m_Title;
        float m_Height;
        Vector2 m_ScrollPosition;
        Func<TraitDefinition, bool> m_DisplayTrait;

        public TraitSelectorPopup(string title, SerializedProperty property, IEnumerable<TraitDefinition> invalidTraits = null,
            Func<TraitDefinition, bool> displayTrait = null)
        {
            m_Property = property;
            m_Title = title;
            m_InvalidTraits = invalidTraits;

            m_Height = Math.Min(Screen.height, TraitAssetDatabase.TraitDefinitions.Count() * 20 + 30);
            m_DisplayTrait = displayTrait;

            if (m_Property.isArray)
            {
                m_Property.ForEachArrayElement(t =>
                {
                    var definition = t.objectReferenceValue as TraitDefinition;
                    if (definition != null && !m_TraitsSelected.Contains(definition))
                    {
                        m_TraitsSelected.Add(definition);
                    }
                });
            }
            else
            {
                var definition = property.objectReferenceValue as TraitDefinition;
                if (definition != null && !m_TraitsSelected.Contains(definition))
                    m_TraitsSelected.Add(definition);
            }
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(180, m_Height);
        }

        public override void OnGUI(Rect rect)
        {
            GUILayout.Label(m_Title, EditorStyles.boldLabel);
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, false, false, GUILayout.Height(rect.height));

            foreach (var trait in TraitAssetDatabase.TraitDefinitions)
            {
                if (m_DisplayTrait != null && !m_DisplayTrait(trait))
                    continue;

                bool selected = m_TraitsSelected.Contains(trait);

                if (!IsValid(trait))
                {
                    GUI.enabled = false;
                }

                bool newSelected = EditorGUILayout.Toggle(trait.name, selected);

                if (!IsValid(trait))
                {
                    newSelected = false;
                }
                GUI.enabled = true;

                if (selected != newSelected)
                {
                    if (selected)
                    {
                        m_TraitsSelected.Remove(trait);
                    }
                    else
                    {
                        if (!m_Property.isArray)
                        {
                            // Single trait selection only
                            m_TraitsSelected.Clear();
                        }

                        m_TraitsSelected.Add(trait);
                    }

                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.EndScrollView();
        }

        bool IsValid(TraitDefinition trait)
        {
            return m_InvalidTraits == null || !m_InvalidTraits.Contains(trait);
        }

        public override void OnClose()
        {
            if (m_Property.isArray)
            {
                bool modified = m_Property.arraySize != m_TraitsSelected.Count;
                if (!modified)
                {
                    for (int i = 0; i < m_Property.arraySize; i++)
                    {
                        if (m_Property.GetArrayElementAtIndex(i).objectReferenceValue != m_TraitsSelected[i])
                        {
                            modified = true;
                            break;
                        }
                    }
                }

                if (modified)
                {
                    m_Property.ClearArray();
                    m_Property.arraySize = m_TraitsSelected.Count;
                    for (var i = 0; i < m_TraitsSelected.Count; i++)
                    {
                        m_Property.GetArrayElementAtIndex(i).objectReferenceValue = m_TraitsSelected[i];
                    }

                    m_Property.serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                var modified = false;
                if (m_TraitsSelected.Count > 0 && m_Property.objectReferenceValue != m_TraitsSelected[0])
                {
                    m_Property.objectReferenceValue = m_TraitsSelected[0];
                    modified = true;
                }
                else if (m_Property.objectReferenceValue != null && m_TraitsSelected.Count == 0)
                {
                    m_Property.objectReferenceValue = null;
                    modified = true;
                }

                if (modified)
                    m_Property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
