using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.Semantic.Traits.Queries;
using UnityEditor.Semantic.Traits.Utility;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Semantic.Traits.Queries.UI
{
    class PropertyFieldNoLabel : PropertyField
    {
        public PropertyFieldNoLabel(SerializedProperty property): base(property, " ")
        {
            RegisterCallback<GeometryChangedEvent>((e) =>
            {
                var labelControl = this.Q<Label>();
                if (labelControl != null)
                    labelControl.style.display = DisplayStyle.None;
            });
        }
    }

    [CustomPropertyDrawer(typeof(IQueryScorer), true)]
    [CustomPropertyDrawer(typeof(IQueryFilter), true)]
    class QueryBlockPropertyDrawer : PropertyDrawer
    {
        const string k_LabelBlockUssClassName = "labelBlock";
        const string k_FieldBlockUssClassName = "fieldBlock";
        const string k_FilterContainerUssClassName = "filterContainer";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            container.AddToClassList(k_FilterContainerUssClassName);
            var queryObjectProperty = property.Copy();

            // There is no way to retrieve a SerializeReference type via the property yet
            // Loop through types that implement QueryNameAttribute to find the AttributeData
            Type queryType = null;
            var queryNameTypes = TypeCache.GetTypesWithAttribute<QueryEditorAttribute>();
            var typeName = queryObjectProperty.GetManagedReferenceTypename();
            foreach (var type in queryNameTypes)
            {
                if (typeName.Equals(type.FullName))
                {
                    queryType = type;
                    break;
                }
            }

            // Display only properties for fields declared as [fieldName] in the description
            if (queryType != null)
            {
                if (queryType.GetCustomAttributes(typeof(QueryEditorAttribute), false)[0] is QueryEditorAttribute queryFormatAttribute)
                {
                    if (queryFormatAttribute.RequiredTraitData != null)
                    {
                        TraitAssetDatabase.Refresh();
                        var def = TraitAssetDatabase.GetTraitDefinitionForType(queryFormatAttribute.RequiredTraitData);
                        container.style.borderLeftColor = def.Color;
                    }

                    var matches = Regex.Matches(queryFormatAttribute.Description, @"\[([^\]]+)\]");
                    int index = 0;
                    foreach (Match match in matches)
                    {
                       var textPart = queryFormatAttribute.Description.Substring(index, match.Index - index);
                       var textPartLabel = new Label(textPart);
                       textPartLabel.AddToClassList(k_LabelBlockUssClassName);
                       container.Add(textPartLabel);

                       index = match.Index + match.Length;

                       var fieldProperty = queryObjectProperty.FindPropertyRelative(match.Value.Substring(1, match.Value.Length - 2));
                       if (fieldProperty != default)
                       {
                           var field = new PropertyFieldNoLabel(fieldProperty);
                           field.AddToClassList(k_FieldBlockUssClassName);
                           field.Bind(queryObjectProperty.serializedObject);
                           container.Add(field);
                       }
                    }

                    if (index < queryFormatAttribute.Description.Length)
                    {
                        var textEndPart = queryFormatAttribute.Description.Substring(index, queryFormatAttribute.Description.Length - index);
                        var textPartLabel = new Label(textEndPart);
                        container.Add(textPartLabel);
                        textPartLabel.AddToClassList(k_LabelBlockUssClassName);
                    }
                }
            }
            else
            {
                container.style.display = DisplayStyle.None;
            }

            return container;
        }
    }

}
