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
    private List<Node> heap = new List<Node>();

    public bool Contains(Node value)
    {
        return heap.Contains(value);
    }

    public int GetCount()
    {
        return heap.Count;
    }

    public void Enqueue(Node value)
    {
        heap.Add(value);
        HeapifyFromEndToBeginning(heap.Count - 1);
    }

    private int HeapifyFromEndToBeginning(int pos)
    {
        if (pos >= heap.Count) return -1;
        while(pos > 0)
        {
            int parentPos = (pos - 1) / 2;
            if (heap[parentPos].CompareTo(heap[pos]) > 0)
            {
                ExchangeElements(parentPos, pos);
                pos = parentPos;
            }
            else
                break;
        }
        return pos;
    }

    private void ExchangeElements(int pos1, int pos2)
    {
        Node val = heap[pos1];
        heap[pos1] = heap[pos2];
        heap[pos2] = val;
    }

    public Node Dequeue()
    {
        if (heap.Count == 0) return null;
        else
        {
            Node result = heap[0];
            DeleteRoot();
            return result;
        }
    }

    private void DeleteRoot()
    {
        if(heap.Count <= 1)
        {
            heap.Clear();
            return;
        }

        heap[0] = heap[heap.Count - 1];
        heap.RemoveAt(heap.Count - 1);

        HeapifyFromBeginningToEnd(0);
    }

    private void HeapifyFromBeginningToEnd(int pos)
    {
        if (pos >= heap.Count) return;
        while (true)
        {
            // on each iteration exchange element with its smallest child
            int smallest = pos;
            int left = 2 * pos + 1;
            int right = 2 * pos + 2;
            if (left < heap.Count && heap[smallest].CompareTo(heap[left]) > 0)
                smallest = left;
            if (right < heap.Count && heap[smallest].CompareTo(heap[right]) > 0)
                smallest = right;

            if (smallest != pos)
            {
                ExchangeElements(smallest, pos);
                pos = smallest;
            }
            else break;
        }
    }
}
