using System;
using System.Linq;
using Unity.Semantic.Traits;
using Unity.Semantic.Traits.Utility;
using UnityEditor.Semantic.Traits.Utility;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Semantic.Traits.UI
{
    [CustomEditor(typeof(EnumDefinition), true)]
    class EnumDefinitionInspector : Editor
    {
        const string k_ElementDefaultName = "NewElement";

        const string k_TraitDynamicStructUxmlFile = "TraitDynamicStructView.uxml";
        const string k_TraitDynamicStructStyleFile = "TraitDynamicStructView.uss";
        const string k_TitleLabelContainerName = "title";

        const string k_PropertyListPropertyPath = "m_Elements";
        const string k_NamePropertyPath = "m_Name";
        const string k_IdPropertyPath = "m_Id";
        const string k_NextIdPropertyPath = "m_NextElementId";

        ReorderableListView m_ReorderableList;
        EnumDefinition m_EnumDefinition;

        void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        }

        public override VisualElement CreateInspectorGUI()
        {
            m_EnumDefinition = target as EnumDefinition;

            var inspectorView = new VisualElement();
            UIUtility.ApplyStyleSheet(inspectorView, k_TraitDynamicStructStyleFile);

            inspectorView.AddToClassList(k_TraitDynamicStructStyleFile);

            var template = UIUtility.LoadTemplate(k_TraitDynamicStructUxmlFile);
            var rootElement = template.CloneTree();
            inspectorView.Add(rootElement);

            var nameLabel = rootElement.Q<Label>(k_TitleLabelContainerName);
            nameLabel.text = "Enumeration values";

            m_ReorderableList = new ReorderableListView();
            m_ReorderableList.addButtonClicked += AddProperty;
            m_ReorderableList.itemRemoved += RemoveProperty;

            inspectorView.Add(m_ReorderableList);

            RePopulateListView();

            return inspectorView;
        }

        void RePopulateListView()
        {
            m_ReorderableList.RemoveAll();

            serializedObject.Update();
            var propertyList = serializedObject.FindProperty(k_PropertyListPropertyPath);
            for (var i = 0; i < propertyList.arraySize; i++)
            {
                var property = propertyList.GetArrayElementAtIndex(i);
                CreateViewForProperty(property);
            }
        }

        void AddProperty()
        {
            serializedObject.Update();
            var nextId = serializedObject.FindProperty(k_NextIdPropertyPath).intValue;

            var serializedListProperty = serializedObject.FindProperty(k_PropertyListPropertyPath);
            var newElement = serializedListProperty.InsertArrayElement();
            newElement.FindPropertyRelative(k_NamePropertyPath).stringValue = ValidateName(k_ElementDefaultName);
            newElement.FindPropertyRelative(k_IdPropertyPath).intValue = nextId;

            serializedObject.FindProperty(k_NextIdPropertyPath).intValue++;
            serializedObject.ApplyModifiedProperties();

            CreateViewForProperty(newElement);
        }

        void RemoveProperty(object itemRemoved)
        {
            serializedObject.Update();
            var propertyList = serializedObject.FindProperty(k_PropertyListPropertyPath);
            var index = propertyList.FindPropertyIndexInArray(p => p.FindPropertyRelative(k_IdPropertyPath).intValue == (int)itemRemoved);
            if (index >= 0)
                propertyList.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
        }

        void ChangePropertyName(SerializedProperty property, string newName)
        {
            serializedObject.Update();
            property.FindPropertyRelative(k_NamePropertyPath).stringValue = ValidateName(newName);
            serializedObject.ApplyModifiedProperties();
        }

        void CreateViewForProperty(SerializedProperty property)
        {
            var id = property.FindPropertyRelative(k_IdPropertyPath).intValue;

            var container = m_ReorderableList.CreateAndAddItemContainer(id);
            var propertyView = new NamedPropertyView(property, k_NamePropertyPath);
            propertyView.nameChanged += ChangePropertyName;

            container.Add(propertyView);
        }

        string ValidateName(string elementName)
        {
            if (elementName.Length < 1)
                elementName = k_ElementDefaultName;

            var validName = TypeResolver.ToTypeNameCase(elementName);
            validName = char.ToUpper(validName[0]) + validName.Substring(1);

            var uniqueName = validName;
            int i = 2;
            while (m_EnumDefinition.Elements.Any(p => p.Name == uniqueName))
            {
                uniqueName = $"{validName}{i++}";
            }

            return uniqueName;
        }

        void OnUndoRedoPerformed()
        {
            RePopulateListView();
        }
    }
}
