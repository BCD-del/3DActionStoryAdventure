// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using MA.Core;
using MA.Core.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using PointerType = UnityEngine.UIElements.PointerType;

namespace MA.UIElements.Editor
{
    class GridView : VisualElement
    {
        enum ScrollingDirection
        {
            None = 0,
            Up,
            Down
        }

        Func<VisualElement> m_MakeItem;
        Action<VisualElement, int> m_BindItem;

        bool m_IsRangeSelectionDirectionUp;

        KeyboardGridNavigationManipulator m_NavigationManipulator;

        int m_RowCount;
        int m_ColumnCount;
        int m_FirstVisibleRowIndex;
        int m_VisibleItemCount;
        int m_RangeSelectionOrigin = -1;
        const int k_ExtraRows = 2;

        float m_FixedItemHeight;
        float m_FixedItemWidth;
        float m_MaximumScrollViewHeight;
        IList m_ItemsSource;

        List<ReusableGridViewRow> m_RowPool;
        List<int> m_ItemsSourceIds;
        ScrollView m_ScrollView;

        const string k_GridViewStyleClassName = "grid-view";
        const string k_GridViewItemsScrollViewStyleClassName = "grid-view-rows";

        Vector2 m_ScrollOffset = Vector2.zero;
        Vector3 m_TouchDownPosition;

        readonly List<int> m_SelectedIndices = new List<int>();
        readonly List<int> m_SelectedIds = new List<int>();
        readonly List<object> m_SelectedItems = new List<object>();
        SelectionType m_SelectionType;

        public event Action<IEnumerable<object>> ItemsChosen;
        public event Action<IEnumerable<object>> SelectionChanged;
        public event Action<IEnumerable<int>> SelectedIndicesChanged;
        public event Action ItemsBuilt;

        public int RowCount => m_RowCount;
        public int ColumnCount => m_ColumnCount;

        public const float DefaultItemSize = 30f;
        public const float ScrollThresholdSquared = 100f;

        public Action<VisualElement, int> UnbindItem { get; set; }

        public Action<VisualElement> DestroyItem { get; set; }

        public Func<VisualElement> MakeItem
        {
            get => m_MakeItem;
            set
            {
                if (m_MakeItem == value)
                    return;
                m_MakeItem = value;
                Rebuild();
            }
        }

        public Action<VisualElement, int> BindItem
        {
            get => m_BindItem;
            set
            {
                if (m_BindItem == value)
                    return;
                m_BindItem = value;
                RefreshItems();
            }
        }

        public float FixedItemHeight
        {
            get => m_FixedItemHeight;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(FixedItemHeight), L10n.Tr("Value needs to be positive for virtualization."));

                float tempVal = value == 0 ? DefaultItemSize : value;

                if (!Mathf.Approximately(m_FixedItemHeight, tempVal))
                {
                    m_FixedItemHeight = tempVal;
                    ComputeGridSize();
                    RefreshItems();
                }
            }
        }

        public float FixedItemWidth
        {
            get => m_FixedItemWidth;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(FixedItemWidth), L10n.Tr("Value needs to be positive for virtualization."));

                float tempVal = value == 0 ? DefaultItemSize : value;

                if (!Mathf.Approximately(m_FixedItemWidth, tempVal))
                {
                    m_FixedItemWidth = tempVal;
                    ComputeGridSize();
                    RefreshItems();
                }
            }
        }

        public int SelectedIndex
        {
            get => m_SelectedIndices.Count == 0 ? -1 : m_SelectedIndices.First();
            set => SetSelection(value);
        }

        public IEnumerable<int> SelectedIndices => m_SelectedIndices;

        public object SelectedItem => m_SelectedItems.Count == 0 ? null : m_SelectedItems.First();

        public IEnumerable<object> SelectedItems => m_SelectedItems;

        public IEnumerable<int> SelectedIds => m_SelectedIds;

        public List<ReusableGridViewItem> ActiveItems => GetActiveItems();

        public int VisibleItemCount => m_VisibleItemCount;

        public int FirstVisibleIndex => m_FirstVisibleRowIndex * m_ColumnCount;

        public int LastVisibleIndex => m_FirstVisibleRowIndex * m_ColumnCount + (m_VisibleItemCount - 1);

        public SelectionType SelectionType
        {
            get => m_SelectionType;
            set
            {
                m_SelectionType = value;
                if (m_SelectionType == SelectionType.None)
                {
                    ClearSelection();
                }
                else if (m_SelectionType == SelectionType.Single)
                {
                    if (m_SelectedIndices.Count > 1)
                    {
                        SetSelection(m_SelectedIndices.First());
                    }
                }
            }
        }

        public IList ItemsSource
        {
            get => m_ItemsSource;
            set
            {
                if (m_ItemsSource is INotifyCollectionChanged oldCollection)
                    oldCollection.CollectionChanged -= OnItemsSourceCollectionChanged;

                m_ItemsSource = value;
                if (m_ItemsSource is INotifyCollectionChanged newCollection)
                    newCollection.CollectionChanged += OnItemsSourceCollectionChanged;

                RefreshItems();
            }
        }

        public GridView(IList itemsSource, float itemFixedWidth, float itemFixedHeight,
            Func<VisualElement> makeItem = null, Action<VisualElement, int> bindItem = null)
        {
            if (itemFixedWidth < 0)
                throw new ArgumentOutOfRangeException(nameof(FixedItemWidth), L10n.Tr("Value needs to be positive for virtualization."));

            if (itemFixedHeight < 0)
                throw new ArgumentOutOfRangeException(nameof(itemFixedHeight), L10n.Tr("Value needs to be positive for virtualization."));

            if (itemFixedWidth == 0)
                itemFixedWidth = DefaultItemSize;

            if (itemFixedHeight == 0)
                itemFixedHeight = DefaultItemSize;

            m_ItemsSource = itemsSource;
            m_FixedItemHeight = itemFixedHeight;
            m_FixedItemWidth = itemFixedWidth;
            m_BindItem = bindItem;
            m_MakeItem = makeItem;

            AddToClassList(k_GridViewStyleClassName);

            m_ScrollView = new ScrollView();
            m_ScrollView.AddToClassList(k_GridViewItemsScrollViewStyleClassName);
            m_ScrollView.verticalScroller.valueChanged += offset => OnScroll(new Vector2(0, offset));

            m_ScrollView.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            m_ScrollView.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

            hierarchy.Add(m_ScrollView);

            m_ScrollView.contentContainer.focusable = true;
            m_ScrollView.contentContainer.usageHints &= ~UsageHints.GroupTransform;
            m_ScrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;

            focusable = true;
#if UNITY_2022_1_OR_NEWER
            delegatesFocus = true;
#endif
            this.SetIsCompositeRoot(true);
        }

        void OnAttachToPanel(AttachToPanelEvent evt)
        {
            if (evt.destinationPanel == null)
                return;

            RegisterCallback<GeometryChangedEvent>(OnSizeChanged);

            m_ScrollView.contentContainer.AddManipulator(m_NavigationManipulator = new KeyboardGridNavigationManipulator(Apply));
            m_ScrollView.RegisterCallback<PointerDownEvent>(OnPointerDown);
            m_ScrollView.RegisterCallback<PointerUpEvent>(OnPointerUp);

            ResetAndBuildItems();
        }

        void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            m_ScrollView.contentContainer.RemoveManipulator(m_NavigationManipulator);
            m_ScrollView.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            m_ScrollView.UnregisterCallback<PointerUpEvent>(OnPointerUp);

            ResetGridViewState();
        }

        void OnPointerDown(PointerDownEvent evt)
        {
            if (!HasValidDataAndBindings() || m_RowPool == null)
                return;

            if (!evt.isPrimary)
                return;

            if (evt.button != (int)MouseButton.LeftMouse)
                return;

            if (evt.pointerType != PointerType.mouse)
            {
                m_TouchDownPosition = evt.position;
                return;
            }

            DoSelect(evt.localPosition, evt.clickCount, evt.actionKey, evt.shiftKey);
        }

        void OnPointerUp(PointerUpEvent evt)
        {
            if (!HasValidDataAndBindings() || m_RowPool == null)
                return;

            if (!evt.isPrimary)
                return;

            if (evt.button != (int)MouseButton.LeftMouse)
                return;

            if (evt.pointerType != PointerType.mouse)
            {
                Vector3 delta = evt.position - m_TouchDownPosition;
                if (delta.sqrMagnitude <= ScrollThresholdSquared)
                    DoSelect(evt.localPosition, evt.clickCount, evt.actionKey, evt.shiftKey);
            }
            else
            {
                int clickedIndex = GetIndexByPosition(evt.localPosition);
                int itemIndex = clickedIndex + m_FirstVisibleRowIndex * m_ColumnCount;
                if (SelectionType == SelectionType.Multiple
                    && !evt.shiftKey
                    && !evt.actionKey
                    && m_SelectedIndices.Count > 1
                    && m_SelectedIndices.Contains(itemIndex))
                {
                    ProcessSingleClick(itemIndex);
                }
            }
        }

        bool Apply(KeyboardGridNavigationManipulator.KeyboardGridNavigationOperation operation, bool shiftKey)
        {
            void HandleSelectionAndScroll(int itemIndex)
            {
                if (SelectionType == SelectionType.Multiple && shiftKey && m_SelectedIndices.Count != 0)
                    DoRangeSelection(itemIndex);
                else
                    SelectedIndex = itemIndex;

                ScrollToItem(itemIndex);
            }

            switch (operation)
            {
                case KeyboardGridNavigationManipulator.KeyboardGridNavigationOperation.None:
                    break;
                case KeyboardGridNavigationManipulator.KeyboardGridNavigationOperation.SelectAll:
                    SelectAll();
                    return true;
                case KeyboardGridNavigationManipulator.KeyboardGridNavigationOperation.Cancel:
                    ClearSelection();
                    return true;
                case KeyboardGridNavigationManipulator.KeyboardGridNavigationOperation.Left:
                    {
                        if (SelectedIndex - 1 < 0)
                            break;

                        int newIndex = Mathf.Max(SelectedIndex - 1, 0);
                        if (newIndex != SelectedIndex)
                        {
                            HandleSelectionAndScroll(newIndex);
                            return true;
                        }
                    }
                    break;
                case KeyboardGridNavigationManipulator.KeyboardGridNavigationOperation.Right:
                    {
                        if (SelectedIndex + 1 >= m_ItemsSource.Count)
                            break;

                        int newIndex = Mathf.Min(SelectedIndex + 1, m_ItemsSource.Count);
                        if (newIndex != SelectedIndex)
                        {
                            HandleSelectionAndScroll(newIndex);
                            return true;
                        }
                    }
                    break;
                case KeyboardGridNavigationManipulator.KeyboardGridNavigationOperation.Up:
                    {
                        if (SelectedIndex - m_ColumnCount < 0)
                            break;

                        int newIndex = Mathf.Max(SelectedIndex - m_ColumnCount, 0);
                        if (newIndex != SelectedIndex)
                        {
                            HandleSelectionAndScroll(newIndex);
                            return true;
                        }
                    }
                    break;
                case KeyboardGridNavigationManipulator.KeyboardGridNavigationOperation.Down:
                    {
                        if (SelectedIndex + m_ColumnCount > m_ItemsSource.Count - 1)
                            break;

                        int newIndex = Mathf.Min(SelectedIndex + m_ColumnCount, m_ItemsSource.Count - 1);
                        if (newIndex != SelectedIndex)
                        {
                            HandleSelectionAndScroll(newIndex);
                            return true;
                        }
                    }
                    break;
                case KeyboardGridNavigationManipulator.KeyboardGridNavigationOperation.Begin:
                    HandleSelectionAndScroll(0);
                    return true;
                case KeyboardGridNavigationManipulator.KeyboardGridNavigationOperation.End:
                    HandleSelectionAndScroll(m_ItemsSource.Count - 1);
                    return true;
                case KeyboardGridNavigationManipulator.KeyboardGridNavigationOperation.PageDown:
                    {
                        if (m_SelectedIndices.Count > 0)
                        {
                            m_RangeSelectionOrigin = m_IsRangeSelectionDirectionUp ? m_SelectedIndices.Min() : m_SelectedIndices.Max();
                            HandleSelectionAndScroll(Mathf.Min(m_ItemsSource.Count - 1, m_RangeSelectionOrigin + (m_VisibleItemCount - 1)));
                        }
                        return true;
                    }
                case KeyboardGridNavigationManipulator.KeyboardGridNavigationOperation.PageUp:
                    {
                        if (m_SelectedIndices.Count > 0)
                        {
                            m_RangeSelectionOrigin = m_IsRangeSelectionDirectionUp ? m_SelectedIndices.Min() : m_SelectedIndices.Max();
                            HandleSelectionAndScroll(Mathf.Max(0, m_RangeSelectionOrigin - (m_VisibleItemCount - 1)));
                        }
                        return true;
                    }
                case KeyboardGridNavigationManipulator.KeyboardGridNavigationOperation.Submit:
                    ItemsChosen?.Invoke(m_SelectedItems);
                    ScrollToItem(SelectedIndex);
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
            }

            return false;
        }

        List<ReusableGridViewItem> GetActiveItems()
        {
            if (m_RowPool == null)
                return null;

            List<ReusableGridViewItem> activeItems = new List<ReusableGridViewItem>();
            foreach (ReusableGridViewRow reusableRow in m_RowPool)
            {
                List<ReusableGridViewItem> items = reusableRow.GetItems();
                if (items == null)
                    continue;

                activeItems.AddRange(items);
            }

            return activeItems;
        }

        public void ScrollToItem(int itemIndex)
        {
            if (!HasValidDataAndBindings() || m_RowPool == null)
                return;

            if (m_RowPool.Count == 0 || m_ColumnCount == 0 || itemIndex < -1)
                return;

            int rowIndex = itemIndex / m_ColumnCount;

            if (itemIndex == -1)
            {
                if (m_ItemsSource.Count < m_VisibleItemCount)
                    m_ScrollView.scrollOffset = new Vector2(0, 0);
                else
                    m_ScrollView.scrollOffset = new Vector2(0, m_MaximumScrollViewHeight);
            }
            else if (itemIndex == m_ItemsSource.Count - 1) // End.
            {
                m_ScrollView.scrollOffset = new Vector2(0, m_MaximumScrollViewHeight);
            }
            else if (itemIndex == 0) // Home.
            {
                m_ScrollView.scrollOffset = new Vector2(0, 0);
            }
            else if (m_FirstVisibleRowIndex >= rowIndex) // Moving up.
            {
                m_ScrollView.scrollOffset = Vector2.up * (m_FixedItemHeight * Mathf.FloorToInt(itemIndex / (float)m_ColumnCount));
            }
            else
            {
                float visibleRowCount = Mathf.Ceil((float)m_VisibleItemCount / m_ColumnCount);
                if (rowIndex < m_FirstVisibleRowIndex + visibleRowCount - 1)
                    return;

                float itemRow = Mathf.Ceil((float)(itemIndex + 1) / m_ColumnCount);
                float yScrollOffset = m_FixedItemHeight * (itemRow - visibleRowCount + 1);

                m_ScrollView.scrollOffset = new Vector2(m_ScrollView.scrollOffset.x, yScrollOffset);
            }

            m_ScrollOffset = m_ScrollView.scrollOffset;
        }

        internal void Apply(KeyboardGridNavigationManipulator.KeyboardGridNavigationOperation operation, EventBase sourceEvent)
        {
            bool shiftKey = sourceEvent is KeyDownEvent kde && kde.shiftKey 
#if UNITY_2022_3_OR_NEWER
                            || sourceEvent is INavigationEvent ne && ne.shiftKey
#endif
;
            if (Apply(operation, shiftKey))
            {
                sourceEvent?.StopPropagation();
            }
        }

        bool HasValidDataAndBindings()
        {
            return m_ItemsSource != null && m_MakeItem != null && m_BindItem != null;
        }

        void NotifyOfSelectionChange()
        {
            if (!HasValidDataAndBindings() || m_RowPool == null)
                return;

            SelectionChanged?.Invoke(m_SelectedItems);
            SelectedIndicesChanged?.Invoke(m_SelectedIndices);
        }

        void DoRangeSelection(int rangeSelectionFinalIndex)
        {
            m_RangeSelectionOrigin = m_IsRangeSelectionDirectionUp ? m_SelectedIndices.Max() : m_SelectedIndices.Min();
            ClearSelectionWithoutValidation();

            List<int> range = new List<int>();
            m_IsRangeSelectionDirectionUp = rangeSelectionFinalIndex < m_RangeSelectionOrigin;
            if (m_IsRangeSelectionDirectionUp)
            {
                for (int i = rangeSelectionFinalIndex; i <= m_RangeSelectionOrigin; i++)
                    range.Add(i);
            }
            else
            {
                for (int i = rangeSelectionFinalIndex; i >= m_RangeSelectionOrigin; i--)
                    range.Add(i);
            }

            AddToSelection(range);
        }

        public void AddToSelection(int index)
        {
            AddToSelection(new[] { index });
        }

        public void AddToSelection(IList<int> indexes)
        {
            if (!HasValidDataAndBindings() || m_RowPool == null || indexes == null || indexes.Count == 0)
                return;

            foreach (int index in indexes)
                AddToSelectionWithoutValidation(index);

            NotifyOfSelectionChange();
        }

        public void RemoveFromSelection(int index)
        {
            if (!HasValidDataAndBindings() || m_RowPool == null)
                return;

            RemoveFromSelectionWithoutValidation(index);
            NotifyOfSelectionChange();
        }

        public void ClearSelection()
        {
            ClearSelectionWithoutNotify();
            NotifyOfSelectionChange();
        }

        void ClearSelectionWithoutValidation()
        {
            foreach (ReusableGridViewItem reusableItem in ActiveItems)
                reusableItem.SetSelected(false);

            m_SelectedIndices.Clear();
            m_SelectedItems.Clear();
            m_SelectedIds.Clear();
        }

        public void ClearSelectionWithoutNotify()
        {
            if (!HasValidDataAndBindings() || m_RowPool == null || m_SelectedIds.Count == 0)
                return;

            ClearSelectionWithoutValidation();
        }

        public void SetSelection(int itemIndex)
        {
            if (itemIndex < 0 || m_ItemsSource == null || itemIndex >= m_ItemsSource.Count)
            {
                ClearSelection();
                return;
            }

            SetSelection(new[] { itemIndex });
        }

        public void SetSelection(IEnumerable<int> indices)
        {
            switch (SelectionType)
            {
                case SelectionType.None:
                    return;
                case SelectionType.Single:
                    if (indices != null)
                        indices = new[] { indices.Last() };
                    break;
                case SelectionType.Multiple:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            SetSelectionInternal(indices, true);
        }

        public void SetSelectionWithoutNotify(IEnumerable<int> indices)
        {
            SetSelectionInternal(indices, false);
        }

        internal void SetSelectionInternal(IEnumerable<int> indices, bool sendNotification)
        {
            if (!HasValidDataAndBindings() || m_RowPool == null || indices == null)
                return;

            ClearSelectionWithoutValidation();

            foreach (int index in indices)
                AddToSelectionWithoutValidation(index);

            if (sendNotification)
                NotifyOfSelectionChange();
        }

        void SelectAll()
        {
            if (!HasValidDataAndBindings() || m_RowPool == null)
                return;

            if (SelectionType != SelectionType.Multiple)
                return;

            for (int itemIndex = 0; itemIndex < m_ItemsSource.Count; itemIndex++)
            {
                object item = m_ItemsSource[itemIndex];
                int id = item.GetHashCode();
                if (!m_SelectedIds.Contains(id))
                {
                    m_SelectedIndices.Add(itemIndex);
                    m_SelectedItems.Add(item);
                    m_SelectedIds.Add(id);
                }
            }

            foreach (ReusableGridViewItem reusableItem in ActiveItems)
                reusableItem.SetSelected(true);

            NotifyOfSelectionChange();
        }

        void AddToSelectionWithoutValidation(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= m_ItemsSource.Count || m_SelectedIndices.Contains(itemIndex))
                return;

            object item = m_ItemsSource[itemIndex];
            m_SelectedIndices.Add(itemIndex);
            m_SelectedItems.Add(item);
            m_SelectedIds.Add(item.GetHashCode());

            int elementIndex = itemIndex - m_FirstVisibleRowIndex * m_ColumnCount;
            if (elementIndex >= ActiveItems.Count || elementIndex < 0)
                return;

            ReusableGridViewItem reusableItem = ActiveItems[elementIndex];
            reusableItem.SetSelected(true);
        }

        void RemoveFromSelectionWithoutValidation(int itemIndex)
        {
            if (!m_SelectedIndices.Contains(itemIndex))
                return;

            object item = m_ItemsSource[itemIndex];
            m_SelectedIndices.Remove(itemIndex);
            m_SelectedItems.Remove(item);
            m_SelectedIds.Remove(item.GetHashCode());

            int elementIndex = itemIndex - m_FirstVisibleRowIndex * m_ColumnCount;
            if (elementIndex >= ActiveItems.Count || elementIndex < 0)
                return;

            ReusableGridViewItem reusableItem = ActiveItems[elementIndex];
            reusableItem.SetSelected(false);
        }

        void DoSelect(Vector2 localPosition, int clickCount, bool actionKey, bool shiftKey)
        {
            int clickedIndex = GetIndexByPosition(localPosition);
            int itemIndex = clickedIndex + m_FirstVisibleRowIndex * m_ColumnCount;

            if (itemIndex > m_ItemsSource.Count - 1 || clickedIndex > m_ItemsSource.Count - 1)
                return;

            switch (clickCount)
            {
                case 1:
                    DoSelectOnSingleClick(itemIndex, actionKey, shiftKey);
                    break;
                case 2:
                    {
                        if (ItemsChosen != null)
                            ProcessSingleClick(itemIndex);

                        ItemsChosen?.Invoke(m_SelectedItems);
                    }
                    break;
            }
        }

        void DoSelectOnSingleClick(int itemIndex, bool actionKey, bool shiftKey)
        {
            if (SelectionType == SelectionType.None)
                return;

            if (SelectionType == SelectionType.Multiple && actionKey)
            {
                m_RangeSelectionOrigin = itemIndex;

                // Add/remove single clicked element
                int id = m_ItemsSourceIds[itemIndex];
                if (m_SelectedIds.Contains(id))
                    RemoveFromSelection(itemIndex);
                else
                    AddToSelection(itemIndex);
            }
            else if (SelectionType == SelectionType.Multiple && shiftKey)
            {
                if (m_RangeSelectionOrigin == -1 || !SelectedItems.Any())
                {
                    m_RangeSelectionOrigin = itemIndex;
                    SetSelection(itemIndex);
                }
                else
                {
                    DoRangeSelection(itemIndex);
                }
            }
            else if (SelectionType == SelectionType.Multiple && m_SelectedIndices.Contains(itemIndex))
            {
                // Do noting, selection will be processed OnPointerUp.
            }
            else // single
            {
                m_RangeSelectionOrigin = itemIndex;
                SetSelection(itemIndex);
            }
        }

        void ProcessSingleClick(int itemIndex)
        {
            m_RangeSelectionOrigin = itemIndex;
            SetSelection(itemIndex);
        }

        internal int GetIndexByPosition(Vector2 localPosition)
        {
            if (m_ColumnCount == 0 || m_RowCount == 0)
                return -1;

            float resolvedRowWidth = m_ScrollView.contentContainer.GetBoundingBox().width;
            float calculatedRowWidth = m_ColumnCount * m_FixedItemWidth;
            float delta = resolvedRowWidth - calculatedRowWidth;
            float extraElementPadding = Mathf.Ceil(delta / (m_ColumnCount - 1));

            float offset = m_ScrollOffset.y - Mathf.FloorToInt(m_ScrollOffset.y / m_FixedItemHeight) * m_FixedItemHeight;
            if (offset == 0)
            {
                int index = Mathf.FloorToInt(localPosition.y / m_FixedItemHeight) * m_ColumnCount + Mathf.FloorToInt(localPosition.x / (m_FixedItemWidth + extraElementPadding));
                if (index >= m_ItemsSource.Count)
                    index = -1;

                return index;
            }

            float visibleOffset = m_FixedItemHeight - offset;
            int visibleRowCount = m_VisibleItemCount / m_ColumnCount;

            float lowerBound = 0f;
            for (int i = 0; i <= visibleRowCount; i++)
            {
                float upperBound = visibleOffset + i * m_FixedItemHeight;
                if (localPosition.y >= lowerBound && localPosition.y < upperBound)
                    return i * m_ColumnCount + Mathf.FloorToInt(localPosition.x / (m_FixedItemWidth + extraElementPadding));
                else
                    lowerBound = upperBound;
            }

            return -1;
        }

        void OnScroll(Vector2 offset)
        {
            int newFirstVisibleRowIndex = (int)(offset.y / m_FixedItemHeight);
            m_ScrollOffset.y = offset.y;

            m_ScrollView.contentContainer.style.paddingTop = newFirstVisibleRowIndex * m_FixedItemHeight;
            m_ScrollView.contentContainer.style.height = m_MaximumScrollViewHeight;

            if (m_FirstVisibleRowIndex == newFirstVisibleRowIndex)
                return;

            ScrollingDirection direction = m_FirstVisibleRowIndex > newFirstVisibleRowIndex ? ScrollingDirection.Up : ScrollingDirection.Down;
            int delta = Math.Abs(newFirstVisibleRowIndex - m_FirstVisibleRowIndex);
            m_FirstVisibleRowIndex = newFirstVisibleRowIndex;
            if (delta >= m_RowCount)
            {
                RebindActiveItems(newFirstVisibleRowIndex);
                return;
            }

            for (int i = 0; i < delta; ++i)
                OnScrollBindItems(direction);
        }

        void RebindActiveItems(int firstVisibleItemIndex)
        {
            int itemIndex = firstVisibleItemIndex * m_ColumnCount;
            foreach (ReusableGridViewItem reusableItem in ActiveItems)
            {
                if (reusableItem.Index < m_ItemsSource.Count && reusableItem.Index != ReusableCollectionItem.UndefinedIndex)
                    DoUnbindItem(reusableItem, reusableItem.Index);

                if (itemIndex >= m_ItemsSource.Count)
                {
                    reusableItem.BindableElement.style.visibility = Visibility.Hidden;
                }
                else
                {
                    DoBindItem(reusableItem, itemIndex, m_ItemsSourceIds[itemIndex]);
                    itemIndex++;
                }
            }
        }

        void OnScrollBindItems(ScrollingDirection scrollingDirection)
        {
            switch (scrollingDirection)
            {
                case ScrollingDirection.None:
                    break;
                case ScrollingDirection.Down:
                    ScrollingDown();
                    break;
                case ScrollingDirection.Up:
                    ScrollingUp();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        void ScrollingDown()
        {
            if (m_RowPool == null || m_RowPool.Count == 0)
                return;

            // When scrolling down, if the last item in the last row is already undefined
            // (because it is already outside the range of source items), then don't bind
            // items from the start.
            int lastIndex = m_RowPool.Last().GetLastItemInRow().Index;
            int nextElementIndexToBind = lastIndex == ReusableCollectionItem.UndefinedIndex ? ReusableCollectionItem.UndefinedIndex : lastIndex + 1;
            ReusableGridViewRow row = m_RowPool.First();
            for (int i = 0; i < m_ColumnCount; i++)
            {
                ReusableGridViewItem reusableItem = row.GetFirstItemInRow();
                row.RemoveItemAt(0);
                DoUnbindItem(reusableItem, reusableItem.Index);

                row.AddItem(reusableItem);
                if (nextElementIndexToBind != ReusableCollectionItem.UndefinedIndex && nextElementIndexToBind < m_ItemsSource.Count)
                {
                    DoBindItem(reusableItem, nextElementIndexToBind, m_ItemsSourceIds[nextElementIndexToBind]);
                    nextElementIndexToBind++;
                }
            }

            m_RowPool.RemoveAt(0);
            m_RowPool.Add(row);
            row.BindableElement.BringToFront();
            row.SetRowVisibility();
        }

        void ScrollingUp()
        {
            if (m_RowPool == null || m_RowPool.Count == 0)
                return;

            int itemIndex = m_RowPool.First().GetFirstItemInRow().Index - 1;
            ReusableGridViewRow row = m_RowPool.Last();
            for (int i = 0; i < m_ColumnCount; i++)
            {
                ReusableGridViewItem reusableItem = row.GetLastItemInRow();
                row.RemoveItemAt(row.BindableElement.childCount - 1);

                if (reusableItem.Index < m_ItemsSource.Count && reusableItem.Index != ReusableCollectionItem.UndefinedIndex)
                    DoUnbindItem(reusableItem, reusableItem.Index);

                row.InsertItemAt(0, reusableItem);
                DoBindItem(reusableItem, itemIndex, m_ItemsSourceIds[itemIndex]);

                itemIndex--;
            }

            m_RowPool.RemoveAt(m_RowPool.Count - 1);
            m_RowPool.Insert(0, row);
            row.BindableElement.SendToBack();
            row.BindableElement.style.display = DisplayStyle.Flex;
        }

        void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            RefreshItems();
        }

        public void RefreshItems()
        {
            if (!HasValidDataAndBindings() || m_RowPool == null || m_ItemsSourceIds == null)
                return;

            m_ItemsSourceIds.Clear();
            foreach (object item in m_ItemsSource)
                m_ItemsSourceIds.Add(item.GetHashCode());

            RefreshSelection();

            ResizeScrollView();
            ResizeColumns();
            ResizeRows();

            ReplaceActiveItems();
        }

        void RefreshSelection()
        {
            m_SelectedIndices.Clear();
            m_SelectedItems.Clear();

            if (m_SelectedIds.Count > 0)
            {
                // Add selected objects to working lists.
                for (int index = 0; index < m_ItemsSource.Count; ++index)
                {
                    if (!m_SelectedIds.Contains(m_ItemsSourceIds[index]))
                        continue;

                    m_SelectedIndices.Add(index);
                    m_SelectedItems.Add(m_ItemsSource[index]);
                }

                m_SelectedIds.Clear();
                foreach (var item in m_SelectedItems)
                    m_SelectedIds.Add(item.GetHashCode());
            }
        }

        void ReplaceActiveItems()
        {
            // Unbind and bind elements in the pool only when necessary.
            int firstVisibleItemIndex = m_FirstVisibleRowIndex * m_ColumnCount;
            int endIndex = firstVisibleItemIndex + ActiveItems.Count;
            int activeItemIndex = 0;
            for (int i = firstVisibleItemIndex; i < endIndex; i++)
            {
                ReusableGridViewItem reusableItem = ActiveItems[activeItemIndex];
                activeItemIndex++;

                if (i >= m_ItemsSource.Count)
                {
                    if (reusableItem.ID != ReusableCollectionItem.UndefinedIndex)
                        DoUnbindItem(reusableItem, reusableItem.Index);

                    continue;
                }

                if (m_ItemsSourceIds[i] == reusableItem.ID)
                    continue;

                DoUnbindItem(reusableItem, i);
            }

            activeItemIndex = 0;
            for (int i = firstVisibleItemIndex; i < endIndex; i++)
            {
                ReusableGridViewItem reusableItem = ActiveItems[activeItemIndex];
                activeItemIndex++;

                if (m_SelectedIds.Contains(reusableItem.ID))
                    reusableItem.SetSelected(true);
                else
                    reusableItem.SetSelected(false);

                if (i >= m_ItemsSource.Count)
                {
                    continue;
                }

                if (m_ItemsSourceIds[i] == reusableItem.ID)
                    continue;
                DoBindItem(reusableItem, i, m_ItemsSourceIds[i]);
            }

            // Hide empty rows that appear in the scrollview.
            foreach (ReusableGridViewRow row in m_RowPool)
                row.SetRowVisibility();
        }

        void ResizeColumns()
        {
            if (m_RowPool == null)
                return;

            int previousColumnCount = m_RowPool.Count > 0 ? m_RowPool[0].BindableElement.childCount : 0;
            if (previousColumnCount > m_ColumnCount) // Column Shrink
            {
                int removeColumnCount = Math.Clamp(previousColumnCount - m_ColumnCount, 0, previousColumnCount);
                foreach (ReusableGridViewRow row in m_RowPool)
                {
                    row.UpdateRow(m_FixedItemWidth, m_FixedItemHeight, m_ColumnCount);
                    for (int i = 0; i < removeColumnCount; i++)
                    {
                        ReusableGridViewItem lastItemInRow = row.GetLastItemInRow();
                        DoUnbindItem(lastItemInRow, lastItemInRow.Index);
                        DestroyItem?.Invoke(lastItemInRow.BindableElement);
                        row.RemoveItem(lastItemInRow.BindableElement);
                    }
                }
            }
            else if (previousColumnCount < m_ColumnCount) // Column Grow
            {
                int addColumnCount = m_ColumnCount - previousColumnCount;
                foreach (ReusableGridViewRow row in m_RowPool)
                {
                    row.UpdateRow(m_FixedItemWidth, m_FixedItemHeight, m_ColumnCount);
                    for (int i = 0; i < addColumnCount; i++)
                        CreateReusableGridViewItem(row);
                }
            }
        }

        void ResizeRows()
        {
            if (m_RowPool == null)
                return;

            int previousRowCount = m_RowPool.Count;
            if (previousRowCount > m_RowCount) // Row Shrink
            {
                int removeRowCount = Math.Clamp(previousRowCount - m_RowCount, 0, previousRowCount);
                for (int i = 0; i < removeRowCount; i++)
                {
                    ReusableGridViewRow reusableRow = m_RowPool.Last();
                    for (int j = 0; j < m_ColumnCount; j++)
                    {
                        ReusableGridViewItem reusableItem = reusableRow.GetLastItemInRow();
                        DoUnbindItem(reusableItem, reusableItem.Index);
                        DestroyItem?.Invoke(reusableItem.BindableElement);
                        reusableRow.RemoveItemAt(reusableRow.BindableElement.childCount - 1);
                    }

                    m_RowPool.RemoveAt(m_RowPool.Count - 1);
                    m_ScrollView.contentContainer.RemoveAt(m_ScrollView.contentContainer.childCount - 1);
                }
            }
            else if (previousRowCount < m_RowCount) // Row Grow
            {
                int addRowCount = m_RowCount - previousRowCount;
                for (int i = 0; i < addRowCount; i++)
                {
                    ReusableGridViewRow row = CreateReusableGridViewRow();
                    for (int j = 0; j < m_ColumnCount; j++)
                        CreateReusableGridViewItem(row);
                }
            }
        }

        void ResizeScrollView()
        {
            int realRowCount = GetRealRowCount(m_ColumnCount);
            m_MaximumScrollViewHeight = realRowCount * m_FixedItemHeight;
            m_ScrollView.contentContainer.style.height = m_MaximumScrollViewHeight;

            int minVisibleItemCount = Mathf.CeilToInt(m_ScrollView.contentViewport.layout.height / m_FixedItemHeight) * m_ColumnCount;
            m_VisibleItemCount = Math.Min(minVisibleItemCount, m_ItemsSource.Count);

            float scrollableHeight = Mathf.Max(0, m_MaximumScrollViewHeight - m_ScrollView.contentViewport.layout.height);
            float scrollOffset = Mathf.Min(m_ScrollOffset.y, scrollableHeight);

            m_ScrollOffset.y = scrollOffset;
            m_FirstVisibleRowIndex = (int)(scrollOffset / m_FixedItemHeight);
            m_ScrollView.verticalScroller.slider.highValue = scrollableHeight;
            m_ScrollView.verticalScroller.slider.value = scrollOffset;
            m_ScrollView.contentContainer.style.paddingTop = m_FirstVisibleRowIndex * m_FixedItemHeight;
        }

        bool CreateReusableGridViewItem(ReusableGridViewRow row)
        {
            VisualElement element = m_MakeItem.Invoke();
            if (element == null)
                return false;

            if (m_RowCount == 1)
                element.style.flexGrow = 1f;

            row.AddItem(element);

            return true;
        }

        ReusableGridViewRow CreateReusableGridViewRow()
        {
            ReusableGridViewRow row = new ReusableGridViewRow();
            row.Init(m_FixedItemWidth, m_FixedItemHeight, m_ColumnCount);
            m_ScrollView.contentContainer.Add(row.BindableElement);
            m_RowPool.Add(row);

            return row;
        }

        void DestroyItems()
        {
            if (m_RowPool == null)
                return;

            foreach (ReusableGridViewItem reusableItem in ActiveItems)
            {
                DoUnbindItem(reusableItem, reusableItem.Index);
                DestroyItem?.Invoke(reusableItem.BindableElement);
            }

            m_RowPool.Clear();
            m_RowPool = null;
        }

        void DoBindItem(ReusableGridViewItem reusableItem, int itemIndex, int id)
        {
            m_BindItem?.Invoke(reusableItem.BindableElement, itemIndex);
            reusableItem.ID = id;
            reusableItem.Index = itemIndex;
            reusableItem.BindableElement.style.visibility = Visibility.Visible;
            reusableItem.BindableElement.style.flexGrow = 0f;

            if (m_SelectedIds.Contains(id))
                reusableItem.SetSelected(true);
        }

        void DoUnbindItem(ReusableGridViewItem reusableItem, int itemIndex)
        {
            int id = reusableItem.ID;
            UnbindItem?.Invoke(reusableItem.BindableElement, itemIndex);
            reusableItem.ID = reusableItem.Index = ReusableCollectionItem.UndefinedIndex;
            reusableItem.BindableElement.style.visibility = Visibility.Hidden;

            if (m_RowCount == 1)
                reusableItem.BindableElement.style.flexGrow = 1f;

            if (m_SelectedIds.Contains(id))
                reusableItem.SetSelected(false);
        }

        public void Rebuild()
        {
            if (m_ItemsSource.Count == 0)
            {
                ResetGridViewState();
                return;
            }

            ResetAndBuildItems();
        }

        void ResetGridViewState()
        {
            m_FirstVisibleRowIndex = 0;
            m_VisibleItemCount = 0;
            m_RowCount = 0;
            m_ColumnCount = 0;
            m_RangeSelectionOrigin = -1;
            m_IsRangeSelectionDirectionUp = false;

            ClearSelectionWithoutNotify();

            DestroyItems();
            m_ScrollView.contentContainer.Clear();
        }

        void ResetAndBuildItems()
        {
            ResetGridViewState();

            float scrollViewWidth = m_ScrollView.contentViewport.resolvedStyle.width;
            float scrollViewHeight = m_ScrollView.contentViewport.resolvedStyle.height;

            // When first attached, the size of the scrollView is NaN.
            if (float.IsNaN(scrollViewWidth) || float.IsNaN(scrollViewHeight))
                return;

            ComputeGridSize(scrollViewWidth, scrollViewHeight);
            BuildItems();
        }

        void OnSizeChanged(GeometryChangedEvent evt)
        {
            if (!HasValidDataAndBindings())
                return;

            if (Mathf.Approximately(evt.newRect.width, evt.oldRect.width) &&
                Mathf.Approximately(evt.newRect.height, evt.oldRect.height))
                return;

            ComputeGridSize();

            if (m_RowPool == null)
                BuildItems();
            else
                RefreshItems();
        }

        void BuildItems()
        {
            if (!HasValidDataAndBindings())
                return;

            if (m_RowCount == 0 || m_ColumnCount == 0)
                return;

            ResizeScrollView();

            m_ItemsSourceIds = new List<int>();
            foreach (object item in m_ItemsSource)
                m_ItemsSourceIds.Add(item.GetHashCode());

            m_RowPool = new List<ReusableGridViewRow>();
            int itemIndex = m_FirstVisibleRowIndex * m_ColumnCount;
            for (int i = 0; i < m_RowCount; i++)
            {
                ReusableGridViewRow row = CreateReusableGridViewRow();
                for (int j = 0; j < m_ColumnCount; j++)
                {
                    if (!CreateReusableGridViewItem(row))
                        continue;

                    ReusableGridViewItem reusableItem = row.GetLastItemInRow();
                    if (itemIndex >= m_ItemsSource.Count)
                    {
                        reusableItem.BindableElement.style.visibility = Visibility.Hidden;
                    }
                    else
                    {
                        DoBindItem(reusableItem, itemIndex, m_ItemsSourceIds[itemIndex]);
                        itemIndex++;
                    }
                }
            }

            OnScroll(m_ScrollOffset);
            ItemsBuilt?.Invoke();
        }

        internal void ComputeGridSize()
        {
            float scrollViewWidth = m_ScrollView.contentViewport.layout.width;
            float scrollViewHeight = m_ScrollView.contentViewport.layout.height;

            // When first attached, the size of the scrollView is NaN.
            if (float.IsNaN(scrollViewWidth) || float.IsNaN(scrollViewHeight))
                return;
            ComputeGridSize(scrollViewWidth, scrollViewHeight);
        }

        internal void ComputeGridSize(float gridViewWidth, float gridViewHeight)
        {
            if (float.IsNaN(gridViewWidth) || gridViewWidth < 0)
                throw new ArgumentOutOfRangeException(nameof(gridViewWidth), "Specified gridview width should be non-negative.");
            if (float.IsNaN(gridViewHeight) || gridViewHeight < 0)
                throw new ArgumentOutOfRangeException(nameof(gridViewHeight), "Specified gridview height should be non-negative.");


            int newColumnCount = Mathf.FloorToInt(gridViewWidth / m_FixedItemWidth);
            int displayableRowCount = Mathf.CeilToInt(gridViewHeight / m_FixedItemHeight) + k_ExtraRows;
            int realRowCount = GetRealRowCount(newColumnCount);
            int newRowCount = Math.Min(realRowCount, displayableRowCount);
            SetGridSize(newColumnCount, newRowCount);
        }

        internal void SetGridSize(int newColumnCount, int newRowCount)
        {
            if (newColumnCount < 0)
                throw new ArgumentOutOfRangeException(nameof(newColumnCount), "Specified column count should be non-negative.");
            if (newRowCount < 0)
                throw new ArgumentOutOfRangeException(nameof(newRowCount), "Specified row count should be non-negative.");

            m_ColumnCount = newColumnCount;
            m_RowCount = newRowCount;
        }

        int GetRealRowCount(int columnCount)
        {
            return columnCount <= 0 ? 0 : Mathf.CeilToInt((float)m_ItemsSource.Count / columnCount);
        }

        internal class ReusableGridViewItem : ReusableCollectionItem
        {
            const string k_GridViewSelectedItemStyleClassName = "grid-view-items__selected";

            public void Init(VisualElement element, float itemWidth, float itemHeight)
            {
                base.Init(element);
                SetupItem(itemWidth, itemHeight);
            }

            public void SetupItem(float itemWidth, float itemHeight)
            {
                BindableElement.style.height = itemHeight;
                BindableElement.style.width = itemWidth;
                BindableElement.style.flexShrink = 0;
                BindableElement.style.visibility = Visibility.Hidden;
            }

            public override void SetSelected(bool selected)
            {
                if (selected)
                    BindableElement.AddToClassList(k_GridViewSelectedItemStyleClassName);
                else
                    BindableElement.RemoveFromClassList(k_GridViewSelectedItemStyleClassName);
            }
        }

        internal class ReusableGridViewRow : ReusableCollectionItem
        {
            float m_ItemHeight;
            float m_ItemWidth;
            int m_MaxItemCount;
            List<ReusableGridViewItem> m_Items;

            public void Init(float itemWidth, float itemHeight, int itemCount)
            {
                m_ItemWidth = itemWidth;
                m_ItemHeight = itemHeight;
                m_MaxItemCount = itemCount;
                m_Items = new List<ReusableGridViewItem>();
                VisualElement row = CreateRow(itemHeight);
                base.Init(row);
            }

            public void UpdateRow(float itemWidth, float itemHeight, int itemCount)
            {
                m_ItemWidth = itemWidth;
                m_ItemHeight = itemHeight;
                m_MaxItemCount = itemCount;
            }

            public VisualElement CreateRow(float itemHeight)
            {
                VisualElement row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.flexShrink = 0;
                row.style.height = itemHeight;
                row.style.justifyContent = Justify.SpaceBetween;
                return row;
            }

            public void AddItem(ReusableGridViewItem reusableItem)
            {
                if (BindableElement.childCount > m_MaxItemCount)
                    return;

                reusableItem.Init(reusableItem.BindableElement, m_ItemWidth, m_ItemHeight);
                m_Items.Add(reusableItem);
                BindableElement.Add(reusableItem.BindableElement);
            }

            public void AddItem(VisualElement element)
            {
                if (BindableElement.childCount > m_MaxItemCount)
                    return;

                ReusableGridViewItem reusableItem = new ReusableGridViewItem();
                reusableItem.Init(element, m_ItemWidth, m_ItemHeight);
                m_Items.Add(reusableItem);
                BindableElement.Add(reusableItem.BindableElement);
            }

            public void RemoveItem(VisualElement element)
            {
                if (m_Items == null)
                    return;

                foreach (ReusableGridViewItem item in m_Items)
                {
                    if (item.BindableElement == element)
                    {
                        m_Items.Remove(item);
                        BindableElement.Remove(element);
                        return;
                    }
                }
            }

            public void RemoveItemAt(int indexInRow)
            {
                if (m_Items == null)
                    return;

                m_Items.RemoveAt(indexInRow);
                BindableElement.RemoveAt(indexInRow);
            }

            public void InsertItemAt(int indexInRow, ReusableGridViewItem item)
            {
                if (BindableElement.childCount > m_MaxItemCount)
                    return;

                m_Items.Insert(indexInRow, item);
                BindableElement.Insert(indexInRow, item.BindableElement);
            }

            public bool IsEmpty()
            {
                if (m_Items == null)
                    return true;

                if (m_Items.Count == 0 || ContainsUnboundItems())
                    return true;

                return false;
            }

            bool ContainsUnboundItems()
            {
                if (m_Items == null)
                    return true;

                foreach (ReusableGridViewItem item in m_Items)
                {
                    if (item.Index == UndefinedIndex)
                        continue;

                    return false;
                }

                return true;
            }

            public void SetRowVisibility()
            {
                if (IsEmpty())
                    BindableElement.style.display = DisplayStyle.None;
                else
                    BindableElement.style.display = DisplayStyle.Flex;
            }

            public List<ReusableGridViewItem> GetItems()
            {
                return m_Items;
            }

            public ReusableGridViewItem GetItemAt(int indexInRow)
            {
                if (m_Items == null || m_Items.Count == 0)
                    return null;

                return m_Items[indexInRow];
            }

            public ReusableGridViewItem GetLastItemInRow()
            {
                if (m_Items == null || m_Items.Count == 0)
                    return null;

                return m_Items.Last();
            }

            public ReusableGridViewItem GetFirstItemInRow()
            {
                if (m_Items == null || m_Items.Count == 0)
                    return null;

                return m_Items.First();
            }
        }

        internal class KeyboardGridNavigationManipulator : Manipulator
        {
            public enum KeyboardGridNavigationOperation
            {
                None = 0,
                SelectAll,
                Cancel,
                Left,
                Right,
                Up,
                Down,
                Begin,
                End,
                PageUp,
                PageDown,
                Submit
            }

            readonly Action<KeyboardGridNavigationOperation, EventBase> m_Action;

            public KeyboardGridNavigationManipulator(Action<KeyboardGridNavigationOperation, EventBase> action)
            {
                m_Action = action;
            }

            protected override void RegisterCallbacksOnTarget()
            {
                target.RegisterCallback<NavigationMoveEvent>(OnNavigationMove);
                target.RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
                target.RegisterCallback<NavigationCancelEvent>(OnNavigationCancel);
                target.RegisterCallback<KeyDownEvent>(OnKeyDown);
            }

            protected override void UnregisterCallbacksFromTarget()
            {
                target.UnregisterCallback<NavigationMoveEvent>(OnNavigationMove);
                target.UnregisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
                target.UnregisterCallback<NavigationCancelEvent>(OnNavigationCancel);
                target.UnregisterCallback<KeyDownEvent>(OnKeyDown);
            }

            internal void OnKeyDown(KeyDownEvent evt)
            {
                // At the moment these actions are not mapped dynamically in the InputSystemEventSystem component.
                // When that becomes the case in the future, remove the following and use corresponding Navigation events.
                KeyboardGridNavigationOperation GetOperation()
                {
                    switch (evt.keyCode)
                    {
                        case KeyCode.A when evt.actionKey: return KeyboardGridNavigationOperation.SelectAll;
                        case KeyCode.Home: return KeyboardGridNavigationOperation.Begin;
                        case KeyCode.End: return KeyboardGridNavigationOperation.End;
                        case KeyCode.PageUp: return KeyboardGridNavigationOperation.PageUp;
                        case KeyCode.PageDown: return KeyboardGridNavigationOperation.PageDown;
                    }
                    return KeyboardGridNavigationOperation.None;
                }

                KeyboardGridNavigationOperation op = GetOperation();
                if (op != KeyboardGridNavigationOperation.None)
                {
                    Invoke(op, evt);
                }
            }

            void OnNavigationSubmit(NavigationSubmitEvent evt)
            {
                Invoke(KeyboardGridNavigationOperation.Submit, evt);
            }

            void OnNavigationCancel(NavigationCancelEvent evt)
            {
                Invoke(KeyboardGridNavigationOperation.Cancel, evt);
            }

            void OnNavigationMove(NavigationMoveEvent evt)
            {
                switch (evt.direction)
                {
                    case NavigationMoveEvent.Direction.Up:
                        Invoke(KeyboardGridNavigationOperation.Up, evt);
                        break;
                    case NavigationMoveEvent.Direction.Down:
                        Invoke(KeyboardGridNavigationOperation.Down, evt);
                        break;
                    case NavigationMoveEvent.Direction.Left:
                        Invoke(KeyboardGridNavigationOperation.Left, evt);
                        break;
                    case NavigationMoveEvent.Direction.Right:
                        Invoke(KeyboardGridNavigationOperation.Right, evt);
                        break;
                }
            }

            void Invoke(KeyboardGridNavigationOperation operation, EventBase evt)
            {
                m_Action?.Invoke(operation, evt);
            }
        }
    }
}