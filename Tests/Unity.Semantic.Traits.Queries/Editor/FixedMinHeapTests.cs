using System;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;

namespace Unity.Semantic.Traits.Queries.Tests.Unit
{
    public class FixedMinHeapTests
    {
        [Test]
        public void InsertedWhenRemainingCapacity()
        {
            var heap = new FixedMinHeap<int>(3, Allocator.Persistent);

            heap.TryInsert(1.0f, 1);
            heap.TryInsert(0.5f, 2);
            heap.TryInsert(0.8f, 3);

            Assert.AreEqual(3, heap.Length);
            Assert.AreEqual(2, heap.Min);
            Assert.AreEqual(1, heap.Max);

            heap.Dispose();
        }

        [Test]
        public void ShrinkWhenValueRemoved()
        {
            var heap = new FixedMinHeap<int>(3, Allocator.Persistent);

            heap.TryInsert(1.0f, 1);
            heap.TryInsert(0.5f, 2);
            heap.TryInsert(0.8f, 3);

            heap.RemoveMin();

            Assert.AreEqual(2, heap.Length);
            Assert.AreEqual(3, heap.Min);
            Assert.AreEqual(1, heap.Max);

            heap.Dispose();
        }

        [Test]
        public void ThrowExceptionWhenRemovingMinInEmptyHeap()
        {
            var heap = new FixedMinHeap<int>(3, Allocator.Persistent);

            heap.TryInsert(1.0f, 1);
            heap.TryInsert(0.5f, 2);
            heap.TryInsert(0.8f, 3);

            heap.RemoveMin();
            Assert.AreEqual(2, heap.Length);
            Assert.AreEqual(3, heap.Min);
            Assert.AreEqual(1, heap.Max);

            heap.RemoveMin();
            Assert.AreEqual(1, heap.Length);
            Assert.AreEqual(1, heap.Min);
            Assert.AreEqual(1, heap.Max);

            heap.RemoveMin();
            Assert.AreEqual(0, heap.Length);

            Assert.Throws<Exception>(
                () => { heap.RemoveMin(); }
                );

            heap.Dispose();
        }

        [Test]
        public void DiscardedWhenCapacityReached()
        {
            var heap = new FixedMinHeap<int>(3, Allocator.Persistent);

            heap.TryInsert(1.0f, 1);
            heap.TryInsert(0.5f, 2);
            heap.TryInsert(0.8f, 3);

            Assert.IsFalse(heap.TryInsert(1.4f, 4));
            Assert.AreEqual(3, heap.Length);
            Assert.AreEqual(2, heap.Min);
            Assert.AreEqual(1, heap.Max);

            heap.Dispose();
        }

        [Test]
        public void ReplacedWhenCapacityReachedWithBetterValue()
        {
            var heap = new FixedMinHeap<int>(3, Allocator.Persistent);

            heap.TryInsert(1.0f, 1);
            heap.TryInsert(0.5f, 2);
            heap.TryInsert(0.8f, 3);

            Assert.IsTrue(heap.TryInsert(0.9f, 4));
            Assert.AreEqual(3, heap.Length);
            Assert.AreEqual(2, heap.Min);
            Assert.AreEqual(4, heap.Max);

            heap.Dispose();
        }

        [Test]
        public void ReplacedWhenCapacityReachedWithBestValue()
        {
            var heap = new FixedMinHeap<int>(3, Allocator.Persistent);

            heap.TryInsert(1.0f, 1);
            heap.TryInsert(0.5f, 2);
            heap.TryInsert(0.8f, 3);

            Assert.IsTrue(heap.TryInsert(0.2f, 4));
            Assert.AreEqual(3, heap.Length);
            Assert.AreEqual(4, heap.Min);
            Assert.AreEqual(3, heap.Max);

            heap.Dispose();
        }
    }
}
