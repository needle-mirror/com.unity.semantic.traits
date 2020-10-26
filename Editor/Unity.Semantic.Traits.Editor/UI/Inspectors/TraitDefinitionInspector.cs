using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Semantic.Traits;
using Unity.Semantic.Traits.Utility;
using UnityEditor.Semantic.Traits.Utility;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Semantic.Traits.UI
{
    [CustomEditor(typeof(TraitDefinition), true)]
    class TraitDefinitionInspector : Editor
    {
        const string k_PropertyDefaultName = "NewProperty";

        const string k_TraitDynamicStructUxmlFile = "TraitDynamicStructView.uxml";
		const string k_TraitDynamicStructStyleFile = "TraitDynamicStructView.uss";
		const string k_TitleLabelContainerName = "title";
        const string k_WarningLabelContainerName = "warning";

        const string k_NamePropertyPath = "m_Name";
        const string k_IdPropertyPath = "m_Id";
        const string k_ValuePropertyPath = "m_Value";
        const string k_PropertyListPropertyPath = "m_Properties";
        const string k_NextIdPropertyPath = "m_NextElementId";

		ReorderableListView m_ReorderableList;
        List<Type> m_PropertyTypes;

        protected void OnEnable()
        {
            TraitAssetDatabase.Refresh();
            m_PropertyTypes = new List<Type>(TypeCache.GetTypesDerivedFrom<TraitPropertyDefinition>());
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var traitDefinition = target as TraitDefinition;

            var inspectorView = new VisualElement();
			UIUtility.ApplyStyleSheet(inspectorView, k_TraitDynamicStructStyleFile);

            inspectorView.AddToClassList(k_TraitDynamicStructStyleFile);

			var template = UIUtility.LoadTemplate(k_TraitDynamicStructUxmlFile);
			var rootElement = template.CloneTree();
            inspectorView.Add(rootElement);

			var nameLabel = rootElement.Q<Label>(k_TitleLabelContainerName);
			nameLabel.text = "Properties";

			m_ReorderableList = new ReorderableListView();
			m_ReorderableList.addButtonClicked += OpenAddPropertyPopup;
			m_ReorderableList.itemRemoved += RemoveProperty;

			inspectorView.Add(m_ReorderableList);

            RePopulateListView();

            var title = new Label("Authoring Settings");
            title.AddToClassList(k_TitleLabelContainerName);
            inspectorView.Add(title);

            var colorField = new ColorField("Color") { showAlpha = false, showEyeDropper = false };
            colorField.SetValueWithoutNotify(traitDefinition.Color);
            colorField.RegisterValueChangedCallback(v =>
            {
                traitDefinition.Color = v.newValue;
                EditorUtility.SetDirty(traitDefinition);
            });
            inspectorView.Add(colorField);

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

		void OpenAddPropertyPopup()
		{
            var addMenu = new GenericMenu();
            foreach (var propertyTypePair in m_PropertyTypes)
            {
                var type = propertyTypePair;

                var typeAttribute = type.GetCustomAttributes(typeof(TraitPropertyTypeAttribute), false).FirstOrDefault();
                if (typeAttribute == null)
                    continue;

                var propertyName = (typeAttribute as TraitPropertyTypeAttribute)?.displayName;
                addMenu.AddItem(new GUIContent(propertyName), false, () =>
                {
                  if (Activator.CreateInstance(type) is TraitPropertyDefinition newTraitProperty)
                  {
                      AddProperty(newTraitProperty);
                  }
                });
            }

            foreach (var enumDefinition in TraitAssetDatabase.EnumDefinitions)
            {
                addMenu.AddItem(new GUIContent($"Enums/{enumDefinition.name}"), false, () =>
                {
                    var newTraitProperty = new EnumReferenceProperty();
                    newTraitProperty.Reference = enumDefinition;

                    AddProperty(newTraitProperty);
                });
            }

            addMenu.ShowAsContext();
        }

        void AddProperty(TraitPropertyDefinition newTraitProperty)
        {
            serializedObject.Update();
            var nextId = serializedObject.FindProperty(k_NextIdPropertyPath).intValue;

            var serializedListProperty = serializedObject.FindProperty(k_PropertyListPropertyPath);
            var newElement = serializedListProperty.InsertArrayElement();
            newElement.managedReferenceValue = newTraitProperty;
            newElement.FindPropertyRelative(k_NamePropertyPath).stringValue = ValidateName(k_PropertyDefaultName);
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
            var typeName = property.GetManagedReferenceTypename();
            var type = m_PropertyTypes.Find(t => t.FullName == typeName);
            if (type == null)
                return;

            var id = property.FindPropertyRelative(k_IdPropertyPath).intValue;
            var container = m_ReorderableList.CreateAndAddItemContainer(id);

            string typeLabel = null;
            var typeAttribute = type.GetCustomAttributes(typeof(TraitPropertyTypeAttribute), false).FirstOrDefault();
            if (typeAttribute != null)
                typeLabel = (typeAttribute as TraitPropertyTypeAttribute)?.displayName;

            var newPropView = new NamedPropertyView(property, k_NamePropertyPath, typeLabel);
            newPropView.nameChanged += ChangePropertyName;

            container.Add(newPropView);

            var valueProperty = property.FindPropertyRelative(k_ValuePropertyPath);
            if (valueProperty != null)
            {
                if (type == typeof(EnumReferenceProperty))
                {
                    // FIXME (replace by property drawers)
                    // Temporary workaround due to bug 1274576 & 1232538
                    CreateEnumReferencePropertyView(property, valueProperty, newPropView);
                }
                else
                {
                    var defaultValue = new PropertyField(valueProperty, "Default");
                    defaultValue.Bind(serializedObject);
                    newPropView.Add(defaultValue);
                }
            }
        }

        void CreateEnumReferencePropertyView(SerializedProperty property, SerializedProperty valueProperty, NamedPropertyView parentView)
        {
            var enumDefinition = property.FindPropertyRelative("m_EnumReference").objectReferenceValue as EnumDefinition;
            if (enumDefinition != null)
            {
                var choices = enumDefinition.Elements.Select(e => e.Id).ToList();
                if (choices.Count > 0)
                {
                    if (!choices.Contains(valueProperty.intValue))
                    {
                        if (valueProperty.intValue > 0)
                            Debug.LogWarning($"{valueProperty.intValue} is not a valid value for enum {enumDefinition.name}.");
                        valueProperty.intValue = choices[0];
                        serializedObject.ApplyModifiedProperties();
                    }

                    var defaultValue = new PopupField<int>("Default", choices, defaultIndex: 0,
                        i => enumDefinition.GetElementNameById(i),
                        i => enumDefinition.GetElementNameById(i));

                    defaultValue.BindProperty(valueProperty);
                    defaultValue.Bind(serializedObject);
                    parentView.Add(defaultValue);
                }
            }
            else
            {
                var warningMessage = new Label("Semantic Enumeration asset is missing");
                warningMessage.AddToClassList(k_WarningLabelContainerName);
                parentView.Add(warningMessage);
            }
        }

        string ValidateName(string elementName)
        {
            var traitDefinition = target as TraitDefinition;

            if (elementName.Length < 1)
                elementName = k_PropertyDefaultName;

            var validName = TypeResolver.ToTypeNameCase(elementName);
            validName = char.ToUpper(validName[0]) + validName.Substring(1);

            var uniqueName = validName;
            int i = 2;
            while (traitDefinition.Properties.Any(p => p.Name == uniqueName))
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
