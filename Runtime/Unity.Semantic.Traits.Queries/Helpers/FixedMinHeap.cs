using System;
using Unity.Collections;
using UnityEngine;

namespace Unity.Semantic.Traits.Queries
{
    /// <summary>
    /// A data structure to store a fixed-size list of elements and their priorities where the root is always the minimum priority value
    /// </summary>
    /// <typeparam name="T">Type of data stored in the heap</typeparam>
    public struct FixedMinHeap<T> : IDisposable where T : struct
    {
        const int k_RootIndex = 0;

        NativeArray<T> m_Data;
        NativeArray<float> m_Priorities;

        int m_Length;
        int m_Capacity;

        int m_MaxPriorityIndex; // Cache current max priority value for faster access
        float m_MaxPriority;

        /// <summary>
        /// Number of elements in the heap
        /// </summary>
        public int Length => m_Length;

        /// <summary>
        /// Maximum capacity
        /// </summary>
        public int Capacity => m_Capacity;

        internal NativeArray<T> Data => m_Data;

        /// <summary>
        /// Constructs a new FixedMinHeap using the specified type of memory allocation.
        /// </summary>
        /// <param name="capacity">The maximum capacity of the heap.</param>
        /// <param name="allocator">A member of the
        /// [Unity.Collections.Allocator](https://docs.unity3d.com/ScriptReference/Unity.Collections.Allocator.html) enumeration.</param>
        public FixedMinHeap(int capacity, Allocator allocator) {

            m_Data = new NativeArray<T>(capacity, allocator);
            m_Priorities = new NativeArray<float>(capacity, allocator);
            m_Length = 0;
            m_Capacity = capacity;

            m_MaxPriority = float.MinValue;
            m_MaxPriorityIndex = -1;
        }

        /// <summary>
        /// Try to insert a value in the heap
        /// </summary>
        /// <param name="priority">Priority value to compare heap elements</param>
        /// <param name="data">Data associated to a help element</param>
        /// <returns>Whether or not the element was inserted in the heap</returns>
        public bool TryInsert(float priority, T data)
        {
            if (Length < Capacity)
            {
                // Insert new value at the end of the tree and bubble up
                m_Data[Length] = data;
                m_Priorities[Length] = priority;

                BubbleUp(Length);
                m_Length++;

                UpdateMaxPriority();
                return true;
            }
            else if (priority < m_MaxPriority)
            {
                // Replace worst value and bubble up
                m_Data[m_MaxPriorityIndex] = data;
                m_Priorities[m_MaxPriorityIndex] = priority;

                BubbleUp(m_MaxPriorityIndex);

                UpdateMaxPriority(true);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the smallest element
        /// </summary>
        public T Min
        {
            get
            {
                if (Length == 0)
                    throw new Exception("Heap is empty");

                return m_Data[k_RootIndex];
            }
        }

        /// <summary>
        /// Get the largest element
        /// </summary>
        public T Max
        {
            get
            {
                if (Length == 0)
                    throw new Exception("Heap is empty");

                return m_Data[m_MaxPriorityIndex];
            }
        }

        /// <summary>
        /// Remove the smallest element
        /// </summary>
        public void RemoveMin()
        {
            if (m_Length == 0)
                throw new Exception("Heap is empty");

            m_Length--;
            m_Data[k_RootIndex] = m_Data[m_Length];
            m_Priorities[k_RootIndex] = m_Priorities[m_Length];
            m_Data[m_Length] = default;
            BubbleDown(k_RootIndex);

            if (m_Length <= 1)
                UpdateMaxPriority(true);
        }

        /// <summary>
        /// Disposes of this container and deallocates its memory immediately.
        /// </summary>
        public void Dispose()
        {
            m_Data.Dispose();
            m_Priorities.Dispose();
        }

        void BubbleUp(int index)
        {
            // For each parent above the child, bubble up value if the parent is smaller
            for (int parent = (index - 1) / 2;
                index != 0 && m_Priorities[index] < m_Priorities[parent];
                index = parent, parent = (index - 1) / 2)
            {
                var data = m_Data[parent];
                var priority = m_Priorities[parent];

                m_Data[parent] = m_Data[index];
                m_Priorities[parent] = m_Priorities[index];

                m_Data[index] = data;
                m_Priorities[index] = priority;
            }
        }

        void BubbleDown(int index)
        {
            // For each child below the parent, bubble down value if the parent is larger
            for (int child = index * 2 + 1;
                child < Length;
                index = child, child = index * 2 + 1)
            {
                if (child + 1 < Length && m_Priorities[child] > m_Priorities[child + 1])
                    child++;

                if (m_Priorities[index] > m_Priorities[child])
                {
                    var data = m_Data[index];
                    var priority = m_Priorities[index];

                    m_Data[index] = m_Data[child];
                    m_Priorities[index] = m_Priorities[child];

                    m_Data[child] = data;
                    m_Priorities[child] = priority;
                }
            }
        }

        void UpdateMaxPriority(bool resetValue = false)
        {
            if (resetValue)
            {
                m_MaxPriority = float.MinValue;
                m_MaxPriorityIndex = -1;
            }

            // Loop trough leaf values to find max priority
            for (int i = Length / 2; i < Length; i++)
            {
                if (m_Priorities[i] >= m_MaxPriority)
                {
                    m_MaxPriority = m_Priorities[i];
                    m_MaxPriorityIndex = i;
                }
            }
        }
    }
}
