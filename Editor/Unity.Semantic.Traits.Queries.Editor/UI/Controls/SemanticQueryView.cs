using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Semantic.Traits.Queries;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Semantic.Traits.Utility;
using ConditionalQuantifiers = Unity.Semantic.Traits.Queries.QueryConditionalGroup.ConditionalQuantifiers;

namespace UnityEditor.Semantic.Traits.Queries.UI
{
    class SemanticQueryView : VisualElement
    {
        const string k_ViewUxmlFile = "SemanticQueryView.uxml";
        const string k_ViewStyleFile = "SemanticQueryView.uss";

        const string k_BlockFieldUssClassName = "blockField";
        const string k_BlockFieldVariantUssClassName = "blockField--variant";
        const string k_SortButtonUssClassName = "sortMethodButton";
        const string k_SortFieldUssClassName = "sortMethodField";
        const string k_SortFieldEmptyUssClassName = "sortMethodEmptyField";
        const string k_SortContainerUssClassName = "sortMethodContainer";
        const string k_GroupHeaderUssClassName = "groupHeader";
        const string k_GroupHeaderLabelUssClassName = "groupHeaderLabel";
        const string k_GroupHeaderButtonUssClassName = "groupHeaderButton";
        const string k_GroupHeaderButtonLabelUssClassName = "groupHeaderButtonLabel";
        const string k_GroupHeaderAddButtonUssClassName = "groupHeaderAddButton";
        const string k_GrowUssClassName = "grow";
        const string k_WithTraitContainerUssClassName = "withTraitContainer";
        const string k_FilterContainerUssClassName = "filterContainer";

        const string k_AddBlockButtonName = "AddBlock";
        const string k_QueryBlockName = "QueryBlock";

        const int k_FilterIdWithTraits = -1;
        const int k_FilterIdWithoutTraits = -2;

        SerializedProperty m_QueryGroupsProperty;

        Dictionary<string, Type> m_QueryTypes;
        Dictionary<string, Type> m_ScorerTypes;

        public SemanticQueryView(SerializedObject serializedObject, string queryPropertyPath)
        {
            var queryProperty = serializedObject.FindProperty(queryPropertyPath);
            m_QueryGroupsProperty = queryProperty.FindPropertyRelative("m_QueryGroups");

            if (m_QueryGroupsProperty.arraySize == 0)
            {
                m_QueryGroupsProperty.InsertArrayElement();
                serializedObject.ApplyModifiedProperties();
            }

            CacheQueryTypes();

            var template = UIUtility.LoadTemplate(k_ViewUxmlFile);
            var clonedTemplate = template.CloneTree();
            hierarchy.Add(clonedTemplate);

            UIUtility.ApplyStyleSheet(clonedTemplate, k_ViewStyleFile);

            var addButton = this.Q<VisualElement>(k_AddBlockButtonName);
            addButton.RegisterCallback<MouseUpEvent>(AddGroupClicked);

            RefreshQueryBlocks();
        }

        void RefreshQueryBlocks()
        {
            var queryBLocks = this.Q<VisualElement>(k_QueryBlockName);
            queryBLocks.Clear();

            var serializedGroups = m_QueryGroupsProperty;
            for (int i = 0; i < serializedGroups.arraySize; i++)
            {
                var groupProperty = serializedGroups.GetArrayElementAtIndex(i);
                DisplayGroup(queryBLocks, i, groupProperty);
            }
        }

        void DisplayGroup(VisualElement queryBLocks, int groupIndex, SerializedProperty groupProperty)
        {
            DrawGroupHeader(queryBLocks, groupProperty, groupIndex);

            var traitFilterProperty = groupProperty.FindPropertyRelative("m_RequiredTraits");

            var traitFilterContainer = new VisualElement();
            traitFilterContainer.AddToClassList(k_BlockFieldUssClassName);
            queryBLocks.Add(traitFilterContainer);

            var traitFilterView = new PropertyField(traitFilterProperty);
            traitFilterView.AddToClassList(k_WithTraitContainerUssClassName);
            traitFilterView.userData = (k_FilterIdWithTraits, traitFilterProperty, groupProperty);
            traitFilterView.RegisterCallback<MouseDownEvent>(BlockContextMenu);
            traitFilterView.Bind(groupProperty.serializedObject);
            traitFilterContainer.Add(traitFilterView);

            traitFilterProperty = groupProperty.FindPropertyRelative("m_ProhibitedTraits");
            if (traitFilterProperty.isExpanded)
            {
                traitFilterContainer = new VisualElement();
                traitFilterContainer.AddToClassList(k_BlockFieldUssClassName);
                queryBLocks.Add(traitFilterContainer);

                traitFilterView = new PropertyField(traitFilterProperty);
                traitFilterView.AddToClassList(k_WithTraitContainerUssClassName);
                traitFilterView.userData = (k_FilterIdWithoutTraits, traitFilterProperty, groupProperty);
                traitFilterView.RegisterCallback<MouseDownEvent>(BlockContextMenu);
                traitFilterView.Bind(groupProperty.serializedObject);
                traitFilterContainer.Add(traitFilterView);
            }

            // Display filter list
            var filterListProperty = groupProperty.FindPropertyRelative("m_Filters");
            for (var j = 0; j < filterListProperty.arraySize; j++)
            {
                var filter = filterListProperty.GetArrayElementAtIndex(j);

                var filterView = new PropertyField(filter);
                filterView.AddToClassList(k_BlockFieldUssClassName);
                filterView.userData = (j, filter, filterListProperty);
                filterView.RegisterCallback<MouseDownEvent>(BlockContextMenu);

                if (j % 2 == 0)
                    filterView.AddToClassList(k_BlockFieldVariantUssClassName);

                filterView.Bind(groupProperty.serializedObject);

                // Add the PropertyField only if a drawer was created successfully
                if (filterView.childCount > 0 && filterView.Children().First().style.display != DisplayStyle.None)
                    queryBLocks.Add(filterView);
            }

            var quantifierProperty = groupProperty.FindPropertyRelative("m_Quantifier");
            if ((ConditionalQuantifiers)quantifierProperty.enumValueIndex == ConditionalQuantifiers.All)
                return;

            var quantifierContainer = new VisualElement();
            quantifierContainer.AddToClassList(k_SortContainerUssClassName);
            queryBLocks.Add(quantifierContainer);

            // Display sort method comparer
            var comparerViewButton = new Button();
            comparerViewButton.AddToClassList(k_SortButtonUssClassName);
            var comparer = groupProperty.FindPropertyRelative("m_QuantifierScorer");
            comparerViewButton.userData = comparer.Copy();
            comparerViewButton.RegisterCallback<MouseUpEvent>(SortMethodContextMenu);

            if (comparer.managedReferenceFullTypename != string.Empty)
            {
                var comparerView = new PropertyField(comparer);
                comparerView.AddToClassList(k_SortFieldUssClassName);
                comparerView.Bind(comparer.serializedObject);
                quantifierContainer.Add(comparerView);
            }
            else
            {
                var comparerView = new Label { text = "No sort method" };
                comparerView.AddToClassList(k_FilterContainerUssClassName);
                comparerView.AddToClassList(k_SortFieldEmptyUssClassName);
                quantifierContainer.Add(comparerView);
            }

            quantifierContainer.Add(comparerViewButton);

        }

        void SortMethodContextMenu(MouseUpEvent evt)
        {
            if (!(evt.currentTarget is VisualElement targetElement))
                return;

            var scorer = (SerializedProperty)targetElement.userData;

            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("None"), false, () =>
            {
                scorer.serializedObject.Update();
                scorer.managedReferenceValue = null;
                scorer.serializedObject.ApplyModifiedProperties();

                RefreshQueryBlocks();
            });
            menu.AddSeparator("");

            AddQueryScorerItem(scorer, menu);

            menu.ShowAsContext();
        }


        void BlockContextMenu(MouseDownEvent evt)
        {
            if (evt.button == 0)
                return;

            if (!(evt.currentTarget is VisualElement targetElement))
                return;

            var groupData = ((int, SerializedProperty, SerializedProperty array))targetElement.userData;
            var group = groupData.Item2;

            var menu = new GenericMenu();

            if (groupData.Item1 == k_FilterIdWithTraits)
            {
                menu.AddDisabledItem(new GUIContent("Remove"));
            }
            if (groupData.Item1 == k_FilterIdWithoutTraits)
            {
                menu.AddItem(new GUIContent("Remove"), false, () =>
                {
                    group.serializedObject.Update();
                    groupData.Item2.FindPropertyRelative("m_Traits").arraySize = 0;
                    groupData.Item2.isExpanded = false;
                    group.serializedObject.ApplyModifiedProperties();

                    RefreshQueryBlocks();
                });
            }
            else
            {
                menu.AddItem(new GUIContent("Remove"), false, () =>
                {
                    group.serializedObject.Update();
                    groupData.array.DeleteArrayElementAtIndex(groupData.Item1);
                    group.serializedObject.ApplyModifiedProperties();

                    RefreshQueryBlocks();
                });
            }

            menu.ShowAsContext();
        }

        void DrawGroupHeader(VisualElement queryBLocks, SerializedProperty groupProperty, int groupIndex)
        {
            var headerContainer = new VisualElement();
            headerContainer.AddToClassList(k_GroupHeaderUssClassName);
            queryBLocks.Add(headerContainer);

            var quantifier = new EnumField(ConditionalQuantifiers.All);
            var quantifierProperty = groupProperty.FindPropertyRelative("m_Quantifier");
            quantifier.BindProperty(quantifierProperty);
            quantifier.RegisterValueChangedCallback(e => RefreshQueryBlocks());

            var quantifierVisualElement = quantifier.Q<VisualElement>(className: EnumField.inputUssClassName);
            quantifierVisualElement.AddToClassList(k_GroupHeaderButtonUssClassName);
            var quantifierLabel = quantifierVisualElement.Q<VisualElement>(className: EnumField.textUssClassName);
            quantifierLabel.AddToClassList(k_GroupHeaderButtonLabelUssClassName);
            headerContainer.Add(quantifier);

            // Input field displayed when quantifier is not 'All'
            var quantifierLimitField = new IntegerField();
            quantifierLimitField.AddToClassList(k_GroupHeaderButtonUssClassName);
            quantifierLimitField.BindProperty(groupProperty.FindPropertyRelative("m_QuantifierLimit"));
            headerContainer.Add(quantifierLimitField);
            quantifierLimitField.style.display = (ConditionalQuantifiers)quantifierProperty.enumValueIndex != ConditionalQuantifiers.All ? DisplayStyle.Flex : DisplayStyle.None;

            quantifierLimitField.RegisterValueChangedCallback(e =>
            {
                if (e.newValue < 1)
                    quantifierLimitField.SetValueWithoutNotify(1);
            });

            var objectLabel = new Label("objects");
            objectLabel.AddToClassList(k_GroupHeaderLabelUssClassName);
            headerContainer.Add(objectLabel);

            var spaceFiller = new VisualElement();
            spaceFiller.AddToClassList(k_GrowUssClassName);
            headerContainer.Add(spaceFiller);

            var filterButton = new Button();
            filterButton.AddToClassList(k_GroupHeaderAddButtonUssClassName);
            filterButton.RegisterCallback<MouseUpEvent>(AddFilterClicked);
            filterButton.userData = (groupIndex, groupProperty);
            headerContainer.Add(filterButton);

        }

        void AddFilterClicked(MouseUpEvent evt)
        {
            if (!(evt.currentTarget is VisualElement targetElement))
                return;

            var groupData = ((int, SerializedProperty))targetElement.userData;
            var group = groupData.Item2;

            var menu = new GenericMenu();

            AddQueryFilterItem(group.FindPropertyRelative("m_Filters"), menu);

            var withoutFilterProperty = group.FindPropertyRelative("m_ProhibitedTraits");
            if (!withoutFilterProperty.isExpanded)
            {
                menu.AddItem(new GUIContent($"Add Filter/Without Traits"), false, () =>
                {
                    group.serializedObject.Update();
                    withoutFilterProperty.isExpanded = true;
                    group.serializedObject.ApplyModifiedProperties();

                    RefreshQueryBlocks();
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Add Filter/Without Traits"));
            }

            menu.AddSeparator("");

            if (m_QueryGroupsProperty.arraySize > 1)
            {
                menu.AddItem(new GUIContent( "Remove Group"), false, () =>
                {

                    group.serializedObject.Update();
                    m_QueryGroupsProperty.DeleteArrayElementAtIndex(groupData.Item1);
                    group.serializedObject.ApplyModifiedProperties();

                    RefreshQueryBlocks();
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent( "Remove Group"));
            }

            menu.ShowAsContext();
        }

        void AddQueryFilterItem(SerializedProperty group, GenericMenu menu, Type ignoreType = null)
        {
            foreach (var queryType in m_QueryTypes.Values)
            {
                if (ignoreType != null && queryType == ignoreType)
                    continue;

                menu.AddItem(new GUIContent($"Add Filter/{GetDisplayName(queryType)}"), false, () =>
                {
                    group.serializedObject.Update();

                    var newQueryProperty = group.InsertArrayElement();
                    newQueryProperty.managedReferenceValue = Activator.CreateInstance(queryType);
                    group.serializedObject.ApplyModifiedProperties();

                    RefreshQueryBlocks();
                });
            }
        }

        void AddQueryScorerItem(SerializedProperty scorer, GenericMenu menu, Type ignoreType = null)
        {
            foreach (var queryType in m_ScorerTypes.Values)
            {
                if (ignoreType != null && queryType == ignoreType)
                    continue;

                menu.AddItem(new GUIContent(GetDisplayName(queryType)), false, () =>
                {
                    scorer.serializedObject.Update();

                    scorer.managedReferenceValue = Activator.CreateInstance(queryType);
                    scorer.serializedObject.ApplyModifiedProperties();

                    RefreshQueryBlocks();
                });
            }
        }

        static string GetDisplayName(Type type)
        {
            var queryNameAttribute = type.GetCustomAttribute<QueryEditorAttribute>();
            var displayName = queryNameAttribute?.Name ??
                ObjectNames.NicifyVariableName(type.Name.Replace("Query", string.Empty));
            return displayName;
        }

        void AddGroupClicked(MouseUpEvent mouseUpEvent)
        {
            m_QueryGroupsProperty.serializedObject.Update();
            var newProperty = m_QueryGroupsProperty.InsertArrayElement();

            // Initialize new group with empty values
            newProperty.FindPropertyRelative("m_Filters").ClearArray();
            newProperty.FindPropertyRelative("m_Quantifier").enumValueIndex = 0;
            newProperty.FindPropertyRelative("m_QuantifierLimit").intValue = 0;
            newProperty.FindPropertyRelative("m_QuantifierScorer").managedReferenceValue = null;
            newProperty.FindPropertyRelative("m_RequiredTraits.m_Traits").ClearArray();

            m_QueryGroupsProperty.serializedObject.ApplyModifiedProperties();

            RefreshQueryBlocks();
        }

        void CacheQueryTypes()
        {
            m_QueryTypes = new Dictionary<string, Type>();
            var queryTypes =  TypeCache.GetTypesDerivedFrom(typeof(IQueryFilter))
                .Where(t => !t.IsGenericType)
                .Where(t => t.IsDefined(typeof(QueryEditorAttribute)));

            foreach (var t in queryTypes)
            {
                m_QueryTypes.Add(t.FullName, t);
            }

            m_ScorerTypes = new Dictionary<string, Type>();
            var scorerTypes =  TypeCache.GetTypesDerivedFrom(typeof(IQueryScorer))
                .Where(t => !t.IsGenericType)
                .Where(t => t.IsDefined(typeof(QueryEditorAttribute)));

            foreach (var t in scorerTypes)
            {
                m_ScorerTypes.Add(t.FullName, t);
            }
        }
    }
}
