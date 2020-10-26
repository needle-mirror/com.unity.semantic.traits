using System;
using System.Linq;
using Unity.Semantic.Traits;
using UnityEditor.Semantic.Traits.Utility;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Semantic.Traits.UI
{
	[CustomEditor(typeof(SemanticObject), false)]
	class SemanticObjectInspector : Editor
	{
        const string k_TraitStyleFile = "TraitLabel.uss";

        static readonly string k_WrapUssClassName = "horizontal-wrap";
        static readonly string k_TraitLabelUssClassName = "traitLabel";
        static readonly string k_TraitAddButtonUssClassName = "traitAddButton";

		SemanticObject m_SemanticObjectComponent;
		float m_TraitAreaHeight;

		protected void OnEnable()
		{
            TraitAssetDatabase.Refresh();
			m_SemanticObjectComponent = ((Component)target).GetComponent<SemanticObject>();
		}

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            UIUtility.ApplyStyleSheet(root, k_TraitStyleFile);
            root.AddToClassList(k_WrapUssClassName);

            var traitComponents = m_SemanticObjectComponent.GetComponents<ITrait>().ToList();
            traitComponents.Sort(((traitA, traitB) => traitA.GetType().Name.CompareTo(traitB.GetType().Name)));

            foreach (var traitComponent in traitComponents)
            {
                var traitType = traitComponent.GetType();
                var definition = TraitAssetDatabase.GetTraitDefinitionForType(traitType);
                if (definition == null)
                {
                    Debug.LogWarning($"Unable to find trait definition for {traitType}");
                    continue;
                }

                var traitLabel = new Label(definition.name);
                traitLabel.AddToClassList(k_TraitLabelUssClassName);
                traitLabel.style.unityBackgroundImageTintColor = definition.Color;
                root.Add(traitLabel);
            }

            var addTraitButton = new Button();
            addTraitButton.ClearClassList();
            addTraitButton.AddToClassList(k_TraitAddButtonUssClassName);
            root.Add(addTraitButton);
            addTraitButton.clicked += () =>
            {
                var menu = new GenericMenu();
                var traitTypes = TypeCache.GetTypesDerivedFrom<ITrait>();
                foreach (var type in traitTypes)
                {
                    var traitName = type.Name;

                    if (!traitComponents.Any(c => c.GetType() == type))
                        menu.AddItem(new GUIContent(traitName), false, () => AddTraitAuthoring(type));
                    else
                        menu.AddDisabledItem(new GUIContent(traitName));
                }

                menu.ShowAsContext();
            };

            return root;
        }

        [MenuItem ("CONTEXT/SemanticObject/Show or hide trait inspectors")]
        static void DoubleMass (MenuCommand command) {
            SemanticObject semanticObject = (SemanticObject)command.context;
            semanticObject.EnableTraitInspectors = !semanticObject.EnableTraitInspectors;

            var authoringComponents = semanticObject.gameObject.GetComponents<ITrait>();
            foreach (var component in authoringComponents)
            {
                ((Component)component).hideFlags = semanticObject.EnableTraitInspectors?HideFlags.None:HideFlags.HideInInspector;
            }

            // Force object components update
            semanticObject.gameObject.SetActive(false);
            semanticObject.gameObject.SetActive(true);
        }

		void AddTraitAuthoring(Type type)
		{
			var traitComponent = m_SemanticObjectComponent.gameObject.AddComponent(type);
			traitComponent.hideFlags = m_SemanticObjectComponent.EnableTraitInspectors?HideFlags.None:HideFlags.HideInInspector;
		}
	}
}
