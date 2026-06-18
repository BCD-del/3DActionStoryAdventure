// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Runtime.CompilerServices;
using MA.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine.Assertions;

namespace MA.Collections
{
    public unsafe struct QuadTree<TElement> : IDisposable where TElement : unmanaged, IEquatable<TElement>
    {
        [NativeDisableUnsafePtrRestriction] TreeData* m_Tree;
        
        enum Quadrant
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }
        
        struct TreeData : IDisposable
        {
            public UnsafeList<Node> Nodes;
            public QuadTree<TElement> ChildTL;
            public QuadTree<TElement> ChildTR;
            public QuadTree<TElement> ChildBL;
            public QuadTree<TElement> ChildBR;
            public AxisAlignedBox2D Bounds;
            public float2 Center;
            public float MinSize;
            public bool IsLeaf;
            public AllocatorManager.AllocatorHandle Allocator;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TreeData(AxisAlignedBox2D bounds, float minSize, AllocatorManager.AllocatorHandle allocator)
            {
                Nodes = new UnsafeList<Node>(k_MaxNodes, allocator);
                ChildTL = ChildTR = ChildBL = ChildBR = default;
                Bounds = bounds;
                Center = bounds.Center;
                MinSize = minSize;
                IsLeaf = true;
                Allocator = allocator;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                if (!Nodes.IsCreated)
                    return;
                
                Nodes.Dispose();
                ChildTL.Dispose();
                ChildTR.Dispose();
                ChildBL.Dispose();
                ChildBR.Dispose();
                Allocator = AllocatorManager.Invalid;
            }
        }
        
        struct Node
        {
            public AxisAlignedBox2D Bounds;
            public TElement Element;
        }
        
        const int k_MaxNodes = 4;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public QuadTree(AxisAlignedBox2D bounds, float minSize, AllocatorManager.AllocatorHandle allocator)
        {
            m_Tree = AllocatorManager.Allocate<TreeData>(allocator);
            *m_Tree = new TreeData(bounds, minSize, allocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (m_Tree == null)
                return;
            
            m_Tree->Dispose();
            AllocatorManager.Free(m_Tree->Allocator, m_Tree);
            m_Tree = null;
        }
        
        public bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Tree != null;
        }
        
        public AxisAlignedBox2D Bounds
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Tree->Bounds;
        }
        
        public bool IsLeaf
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Tree->IsLeaf;
        }
        
        public QuadTree<TElement> this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (&m_Tree->ChildBL)[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set => (&m_Tree->ChildBL)[index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            for (int i = 0; i < 4; i++)
            {
                if (!this[i].IsCreated)
                    continue;
                
                this[i].Dispose();
                this[i] = default;
            }
            
            m_Tree->Nodes.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(in TElement element, AxisAlignedBox2D bounds)
        {
            InsertElementRecursive(element, bounds);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in TElement element, AxisAlignedBox2D bounds)
        {
            Span<QuadTree<TElement>> trees = stackalloc QuadTree<TElement>[4];
            int count = GetQuads(bounds, trees);

            bool removed = RemoveNodeForElement(element);
            if (removed)
                return true;

            for (int treeIndex = 0; treeIndex < count; treeIndex++)
            {
                removed = trees[treeIndex].Remove(element, bounds);
                if (removed)
                    break;
            }
            
            return removed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetElementsInBounds(AxisAlignedBox2D bounds, NativeList<TElement> elements)
        {
            elements.ReserveAdditional(m_Tree->Nodes.Length);
            for (int i = 0; i < m_Tree->Nodes.Length; i++)
            {
                Node node = m_Tree->Nodes[i];
                if (node.Bounds.Overlaps(bounds))
                    elements.Add(node.Element);
            }
        }

        bool RemoveNodeForElement(in TElement element)
        {
            int index = -1;
            for (int i = 0; i < m_Tree->Nodes.Length; i++)
            {
                if (m_Tree->Nodes[i].Element.Equals(element))
                {
                    index = i;
                    break;
                }
            }
            
            if (index != -1)
            {
                m_Tree->Nodes.RemoveAtSwapBack(index);
                return true;
            }
            
            return false;
        }
        
        void InsertElementRecursive(in TElement element, AxisAlignedBox2D bounds)
        {
            Span<QuadTree<TElement>> trees = stackalloc QuadTree<TElement>[4];
            int count = GetQuads(bounds, trees);
            if (count == 0)
            {
                Assert.IsTrue(m_Tree->IsLeaf);
                bool canSplit = m_Tree->Bounds.DiagonalLengthSq > math.lengthsq(m_Tree->MinSize);
                if (!canSplit || m_Tree->Nodes.Length < k_MaxNodes)
                {
                    m_Tree->Nodes.Add(new Node { Bounds = bounds, Element = element });
                }
                else
                {
                    // At capacity, so split and try again
                    Split();
                    InsertElementRecursive(element, bounds);
                }
            }
            else if (count == 1)
            {
                // Fully contained, insert it there
                trees[0].InsertElementRecursive(element, bounds);
            }
            else
            {
                // Overlaps multiple subtrees, store here
                m_Tree->Nodes.Add(new Node { Bounds = bounds, Element = element });
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Split()
        {
            Assert.IsTrue(m_Tree->IsLeaf);
            
            float2 extents = m_Tree->Bounds.Extents;
            float2 xExtents = new float2(extents.x, 0);
            float2 yExtents = new float2(0, extents.y);
            
            float2 center = m_Tree->Center;
            float2 topMid = center + yExtents;
            float2 bottomMid = center - yExtents;
            float2 leftMid = center - xExtents;
            float2 rightMid = center + xExtents;
            float2 bottomLeft = center - extents;
            float2 topRight = center + extents;
            
            m_Tree->ChildTL = new QuadTree<TElement>(new AxisAlignedBox2D(leftMid, topMid), m_Tree->MinSize, m_Tree->Allocator);
            m_Tree->ChildTR = new QuadTree<TElement>(new AxisAlignedBox2D(center, topRight), m_Tree->MinSize, m_Tree->Allocator);
            m_Tree->ChildBL = new QuadTree<TElement>(new AxisAlignedBox2D(bottomLeft, center), m_Tree->MinSize, m_Tree->Allocator);
            m_Tree->ChildBR = new QuadTree<TElement>(new AxisAlignedBox2D(bottomMid, rightMid), m_Tree->MinSize, m_Tree->Allocator);
            m_Tree->IsLeaf = false;
            
            UnsafeList<Node> overlapping = new UnsafeList<Node>(m_Tree->Nodes.Length, m_Tree->Allocator);
            Span<QuadTree<TElement>> trees = stackalloc QuadTree<TElement>[4];
            for (int i = 0; i < m_Tree->Nodes.Length; i++)
            {
                Node node = m_Tree->Nodes[i];
                int count = GetQuads(node.Bounds, trees);
                if (count == 1)
                {
                    trees[0].m_Tree->Nodes.Add(node);
                }
                else
                {
                    overlapping.Add(node);
                }
            }

            m_Tree->Nodes.Dispose();
            m_Tree->Nodes = overlapping;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int GetQuads(AxisAlignedBox2D bounds, Span<QuadTree<TElement>> trees)
        {
            int count = 0;
            
            if (!m_Tree->IsLeaf)
            {
                bool2 neg = bounds.Min <= m_Tree->Center;
                bool2 pos = bounds.Max >= m_Tree->Center;

                if (neg.x && neg.y)
                    trees[count++] = this[(int)Quadrant.BottomLeft];
                if (pos.x && neg.y)
                    trees[count++] = this[(int)Quadrant.BottomRight];
                if (neg.x && pos.y)
                    trees[count++] = this[(int)Quadrant.TopLeft];
                if (pos.x && pos.y)
                    trees[count++] = this[(int)Quadrant.TopRight];
            }

            return count;
        }
    }
}