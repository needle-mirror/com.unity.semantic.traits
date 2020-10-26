using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Semantic.Traits.UI
{
	class ReorderableListItem : VisualElement
	{
		static readonly string k_ItemContainerUssClassName = "listItem";
		static readonly string k_DragHandleUssClassName = "dragHandle";
		static readonly string k_ItemVariantUssClassName = "reorderableItem--variant";

		public object Item;
		int m_Index;

		public int Index
		{
			get => m_Index;
			set
			{
				m_Index = value;
				if (m_Index % 2 != 0)
					AddToClassList(k_ItemVariantUssClassName);
				else
					RemoveFromClassList(k_ItemVariantUssClassName);
			}
		}

		public ReorderableListItem(int index)
		{
			Index = index;
			AddToClassList(k_ItemContainerUssClassName);

			var handle = new VisualElement();
			handle.AddToClassList(k_DragHandleUssClassName);
			Add(handle);
		}
	}

	class ReorderableListView : BindableElement
	{
		const string k_ListUxmlFile = "ReorderableListView.uxml";
		const string k_ListStyleFile = "ReorderableListView.uss";

		static readonly string k_ItemUssClassName = "reorderableItem";
		static readonly string k_ItemSelectedUssClassName = k_ItemUssClassName + "--selected";

		static readonly string k_AddButtonName = "addButton";
		static readonly string k_RemoveButtonName = "removeButton";
		static readonly string k_ItemsRootName = "itemsRoot";
		static readonly string k_EmptyMessageName = "emptyMessage";

		public event Action addButtonClicked;
		public event Action<object> itemRemoved;

		VisualElement m_RootList;
		VisualElement m_EmptyLabel;
		Button m_RemoveButton;

		int m_SelectedItem = -1;

		public ReorderableListView()
		{
			var template = UIUtility.LoadTemplate(k_ListUxmlFile);
			var clonedTemplate = template.CloneTree();
			hierarchy.Add(clonedTemplate);

			var addButton = this.Q<Button>(k_AddButtonName);
			addButton.clicked += AddButtonClicked;

			m_RemoveButton = this.Q<Button>(k_RemoveButtonName);
			m_RemoveButton.clicked += RemoveButtonClicked;
			m_RemoveButton.SetEnabled(false);

			m_RootList = this.Q<VisualElement>(k_ItemsRootName);
			m_EmptyLabel = this.Q<VisualElement>(k_EmptyMessageName);

			UIUtility.ApplyStyleSheet(clonedTemplate, k_ListStyleFile);
		}

        internal ReorderableListItem CreateAndAddItemContainer(object item)
        {
            var itemContainer = new ReorderableListItem(m_RootList.childCount);
            itemContainer.AddToClassList(k_ItemUssClassName);
            itemContainer.AddManipulator(new Clickable((a) => SelectItem(((ReorderableListItem)a.currentTarget).Index)));
            itemContainer.Item = item;

            m_RootList.Add(itemContainer);

            if (m_RootList.childCount > 0)
                m_EmptyLabel.style.display = DisplayStyle.None;

            return itemContainer;
        }

        internal void RemoveAll()
        {
            m_SelectedItem = -1;
            m_RootList.Clear();
        }

		void AddButtonClicked()
		{
			addButtonClicked?.Invoke();
		}

		void RemoveButtonClicked()
		{
			if (m_SelectedItem < 0)
				return;

			var toRemove = m_RootList[m_SelectedItem];
			var removedObject = (toRemove as ReorderableListItem)?.Item;
			m_RootList.Remove(toRemove);

			if (m_RootList.childCount == 0)
				m_EmptyLabel.style.display = DisplayStyle.Flex;

			// Reassign indexes
			for (int i = 0; i < m_RootList.childCount; i++)
			{
				var item = m_RootList[i] as ReorderableListItem;
				item.Index = i;
			}

			// Try Select another valid item
			SelectItem(m_SelectedItem);

			itemRemoved?.Invoke(removedObject);
		}

		void SelectItem(int index)
		{
			if (m_SelectedItem != -1 && m_SelectedItem < m_RootList.childCount)
				m_RootList[m_SelectedItem].RemoveFromClassList(k_ItemSelectedUssClassName);

			m_SelectedItem = index;

			if (index < m_RootList.childCount)
				m_RootList[m_SelectedItem].AddToClassList(k_ItemSelectedUssClassName);
			else
				m_SelectedItem = -1;

			m_RemoveButton.SetEnabled(m_SelectedItem != -1);
		}
    }
}
