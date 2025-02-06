using UnityEngine;

using System;
using System.Collections.Generic;

// 实现优先队列类
/*最小堆*/
public class PriorityQueue<T> where T : IComparable<T>
{
    private List<T> heap;

    public PriorityQueue()
    {
        heap = new List<T>();
    }

    public void EnqueueWithList(List<T> list)
    {
        foreach(T item in list)
        {
            Enqueue(item);
        }
    }

    // 入队操作
    public void Enqueue(T item)
    {
        heap.Add(item);
        int currentIndex = heap.Count - 1;
        while (currentIndex > 0)
        {
            int parentIndex = (currentIndex - 1) / 2;
            if (heap[currentIndex].CompareTo(heap[parentIndex]) >= 0) break;
            Swap(currentIndex, parentIndex);
            currentIndex = parentIndex;
        }
    }

    // 出队操作
    public T Dequeue()
    {
        if (heap.Count == 0)
        {
            throw new InvalidOperationException("优先队列为空");
        }
        T result = heap[0];
        int lastIndex = heap.Count - 1;
        heap[0] = heap[lastIndex];
        heap.RemoveAt(lastIndex);
        lastIndex--;
        int currentIndex = 0;
        while (true)
        {
            int leftChildIndex = 2 * currentIndex + 1;
            int rightChildIndex = 2 * currentIndex + 2;
            int smallestIndex = currentIndex;
            if (leftChildIndex <= lastIndex && heap[leftChildIndex].CompareTo(heap[smallestIndex]) < 0)
            {
                smallestIndex = leftChildIndex;
            }
            if (rightChildIndex <= lastIndex && heap[rightChildIndex].CompareTo(heap[smallestIndex]) < 0)
            {
                smallestIndex = rightChildIndex;
            }
            if (smallestIndex == currentIndex) break;
            Swap(currentIndex, smallestIndex);
            currentIndex = smallestIndex;
        }
        return result;
    }

    public void Clear()
    {
        while(heap.Count > 0)
        {
            Dequeue();
        }
    }

    public bool Empty()
    {
        return Count == 0;
    }
    // 获取队列中元素的数量
    public int Count
    {
        get { return heap.Count; }
    }

    // 交换两个元素的位置
    private void Swap(int index1, int index2)
    {
        T temp = heap[index1];
        heap[index1] = heap[index2];
        heap[index2] = temp;
    }
}