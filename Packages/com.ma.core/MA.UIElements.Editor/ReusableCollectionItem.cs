// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace MA.UIElements.Editor
{
    class ReusableCollectionItem
    {
        public const int UndefinedIndex = -1;

        public virtual VisualElement RootElement => BindableElement;
        
        public VisualElement BindableElement { get; protected set; }
        
        public ValueAnimation<StyleValues> Animator { get; set; }

        public int Index { get; set; }
        public int ID { get; set; }
        internal bool IsDragGhost { get; private set; } // Identifies an item as an invisible duplicate of the dragged item.

        public event Action<ReusableCollectionItem> GeometryChanged;

        internal event Action<ReusableCollectionItem> Destroyed;

        protected EventCallback<GeometryChangedEvent> m_GeometryChangedEventCallback;

        public ReusableCollectionItem()
        {
            Index = ID = UndefinedIndex;
            m_GeometryChangedEventCallback = OnGeometryChanged;
        }

        public virtual void Init(VisualElement item)
        {
            BindableElement = item;
        }

        public virtual void PreAttachElement()
        {
            RootElement.AddToClassList(BaseVerticalCollectionView.itemUssClassName);
            RootElement.RegisterCallback(m_GeometryChangedEventCallback);
        }

        public virtual void DetachElement()
        {
            RootElement.RemoveFromClassList(BaseVerticalCollectionView.itemUssClassName);
            RootElement.UnregisterCallback(m_GeometryChangedEventCallback);

            RootElement?.RemoveFromHierarchy();
            SetSelected(false);
            SetDragGhost(false);
            Index = ID = UndefinedIndex;
        }

        public virtual void DestroyElement()
        {
            Destroyed?.Invoke(this);
        }

        public virtual void SetSelected(bool selected)
        {
            if (selected)
            {
                RootElement.AddToClassList(BaseVerticalCollectionView.itemSelectedVariantUssClassName);
                RootElement.AddPseudoState(PseudoStates.Checked);
            }
            else
            {
                RootElement.RemoveFromClassList(BaseVerticalCollectionView.itemSelectedVariantUssClassName);
                RootElement.RemovePseudoState(PseudoStates.Checked);
            }
        }

        public virtual void SetDragGhost(bool dragGhost)
        {
            IsDragGhost = dragGhost;
            RootElement.style.maxHeight = IsDragGhost ? 0 : StyleKeyword.Initial;
            BindableElement.style.display = IsDragGhost ? DisplayStyle.None : DisplayStyle.Flex;
        }

        protected void OnGeometryChanged(GeometryChangedEvent evt)
        {
            GeometryChanged?.Invoke(this);
        }
    }
}