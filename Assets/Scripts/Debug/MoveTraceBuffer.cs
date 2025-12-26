using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveTraceBuffer
{
    private readonly int capacity;
    private readonly Queue<MoveTraceEntry> buffer;

    public MoveTraceBuffer(int capacity = 300)
    {
        this.capacity = capacity;
        buffer = new Queue<MoveTraceEntry>(capacity);
    }

    public void Add(MoveTraceEntry entry)
    {
        if (buffer.Count >= capacity)
        {
            buffer.Dequeue();
        }

        buffer.Enqueue(entry);
    }

    public List<MoveTraceEntry> Snapshot()
    {
        return buffer.ToList();
    }

    public void Clear() => buffer.Clear();
}
