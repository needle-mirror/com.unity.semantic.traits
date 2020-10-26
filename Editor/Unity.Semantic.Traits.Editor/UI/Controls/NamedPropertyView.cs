using System;
using Unity.Semantic.Traits;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Semantic.Traits.UI
{
    sealed class NamedPropertyView : VisualElement
    {
        const string k_PropertyUxmlFile = "TraitPropertyView.uxml";
        const string k_PropertyStyleFile = "TraitPropertyView.uss";

        const string k_NameLabelElementName = "nameLabel";
        const string k_TypeLabelElementName = "typeLabel";
        const string k_NameFieldElementName = "nameTextField";
        const string k_GrowUssClassName = "grow";

        public event Action<SerializedProperty, string> nameChanged;

        readonly SerializedProperty m_Property;
        readonly string m_BindingPath;
        readonly Label m_NameLabel;
        readonly Label m_TypeLabel;
        readonly TextField m_NameField;

        public NamedPropertyView(SerializedProperty property, string path, string propertyTypeLabel = null)
        {
            m_Property = property;
            m_BindingPath = path;

            UIUtility.ApplyStyleSheet(this, k_PropertyStyleFile);
            AddToClassList(k_GrowUssClassName);

            var template = UIUtility.LoadTemplate(k_PropertyUxmlFile);
            var rootElement = template.CloneTree();
            Add(rootElement);

            m_NameLabel = rootElement.Q<Label>(k_NameLabelElementName);
            m_NameLabel.focusable = true;

            m_TypeLabel = rootElement.Q<Label>(k_TypeLabelElementName);

            m_NameField = rootElement.Q<TextField>(k_NameFieldElementName);
            m_NameField.isDelayed = true;
            m_NameField.focusable = true;

            m_NameLabel.text = m_Property.FindPropertyRelative(path)?.stringValue;

            if (propertyTypeLabel != null)
                m_TypeLabel.text = $"({propertyTypeLabel})";

            m_NameField.SetValueWithoutNotify(m_NameLabel.text);

            SetNameEditing(false);

            m_NameLabel.RegisterCallback<MouseDownEvent>(OnNameLabelMouseUp);
            m_NameField.RegisterCallback<ChangeEvent<string>>(OnPropertyNameChanged);
            m_NameField.RegisterCallback<FocusOutEvent>(OnNameFieldLostFocus);

            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        void OnNameFieldLostFocus(FocusOutEvent evt)
        {
            SetNameEditing(false);
        }

        void OnPropertyNameChanged(ChangeEvent<string> evt)
        {
            if (m_Property != null)
            {
                nameChanged?.Invoke(m_Property, evt.newValue);

                m_NameLabel.text = m_Property.FindPropertyRelative(m_BindingPath).stringValue;
                m_NameField.SetValueWithoutNotify(m_NameLabel.text);
            }

            SetNameEditing(false);
        }

        void OnNameLabelMouseUp(MouseDownEvent evt)
        {
            if (evt.button != (int) MouseButton.LeftMouse || evt.modifiers != EventModifiers.None)
                return;

            if (evt.clickCount == 1)
                SetNameEditing(true);
        }

        void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            nameChanged = null;
        }

        void SetNameEditing(bool editName)
        {
            var showStyle = DisplayStyle.Flex;
            var hideStyle = DisplayStyle.None;

            m_NameField.style.display = editName ? showStyle : hideStyle;
            m_NameLabel.style.display = editName ? hideStyle : showStyle;
            m_TypeLabel.style.display = editName ? hideStyle : showStyle;

            if (editName)
                m_NameField.schedule.Execute(() => m_NameField.Q("unity-text-input").Focus());
        }
    }
}
