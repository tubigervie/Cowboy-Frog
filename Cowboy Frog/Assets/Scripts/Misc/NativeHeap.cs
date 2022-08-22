using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Priority Queue implemented as a Binary Heap.
/// 
/// Push - O(log(n))
/// Pop - O(Log(n))
/// 
/// </summary>
public class NativeHeap
{
    private Node[] heap;
    int currentItemCount;

    public NativeHeap(int maxHeapSize)
    {
        heap = new Node[maxHeapSize];
        currentItemCount = 0;
    }

    public int GetCount()
    {
        return currentItemCount;
    }

    public bool Contains(Node value)
    {
        return value == heap[value.heapIndex];
    }

    public void Enqueue(Node value)
    {
        value.heapIndex = currentItemCount;
        heap[currentItemCount] = value;
        SortUp(value);
        currentItemCount++;
    }

    void SortUp(Node item)
    {
        int parentIndex = (item.heapIndex - 1) / 2;
        while(true)
        {
            Node parentItem = heap[parentIndex];
            if(item.CompareTo(parentItem) < 0)
            {
                Swap(item, parentItem);
            }
            else
            {
                break;
            }
            parentIndex = (item.heapIndex - 1) / 2;
        }
    }

    void Swap(Node itemA, Node itemB)
    {
        heap[itemA.heapIndex] = itemB;
        heap[itemB.heapIndex] = itemA;
        int itemAIndex = itemA.heapIndex;
        itemA.heapIndex = itemB.heapIndex;
        itemB.heapIndex = itemAIndex;
    }

    public Node Dequeue()
    {
        if (currentItemCount == 0) return null;
        else
        {
            Node firstTime = heap[0];
            currentItemCount--;
            heap[0] = heap[currentItemCount];
            heap[0].heapIndex = 0;
            SortDown(heap[0]);
            return firstTime;
        }
    }

    void SortDown(Node item)
    {
        while (true)
        {
            int childIndexLeft = item.heapIndex * 2 + 1;
            int childIndexRight = item.heapIndex * 2 + 2;
            int swapIndex = 0;
            if (childIndexLeft < currentItemCount)
            {
                swapIndex = childIndexLeft;
                if (childIndexRight < currentItemCount)
                {
                    if (heap[childIndexLeft].CompareTo(heap[childIndexRight]) > 0)
                    {
                        swapIndex = childIndexRight;
                    }
                }
                if (item.CompareTo(heap[swapIndex]) > 0)
                {
                    Swap(item, heap[swapIndex]);
                }
                else
                    return;
            }
            else
                return;
        }
    }

}
