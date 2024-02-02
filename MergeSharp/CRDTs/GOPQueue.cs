using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using MergeSharp;

[TypeAntiEntropyProtocol(typeof(GPriorityQueue<>))]
public class GPriorityQueueMsg<T> : PropagationMessage
{
    [JsonInclude]
    public SortedDictionary<int, T> priorityQueue;

    public GPriorityQueueMsg()
    {
    }

    public GPriorityQueueMsg(SortedDictionary<int, T> priorityQueue)
    {
        this.priorityQueue = priorityQueue;
    }

    public override void Decode(byte[] input)
    {
        var json = JsonSerializer.Deserialize<GPriorityQueueMsg<T>>(input);
        this.priorityQueue = json.priorityQueue;
    }

    public override byte[] Encode()
    {
        return JsonSerializer.SerializeToUtf8Bytes(this);
    }
}

[ReplicatedType("GPriorityQueue")]
public class GPriorityQueue<T> : CRDT
{
    private SortedDictionary<int, T> _priorityQueue;

    public GPriorityQueue()
    {
        this._priorityQueue = new SortedDictionary<int, T>();
    }

    [OperationType(OpType.Update)]
    public virtual void Add(int priority, T item)
    {
        _priorityQueue[priority] = item;
    }

    public IEnumerable<T> GetElementsInPriorityOrder()
    {
        return _priorityQueue.Values;
    }

    public void Merge(GPriorityQueueMsg<T> received)
    {
        foreach (var item in received.priorityQueue)
        {
            if (!_priorityQueue.ContainsKey(item.Key) || !EqualityComparer<T>.Default.Equals(_priorityQueue[item.Key], item.Value))
            {
                _priorityQueue[item.Key] = item.Value;
            }
        }
    }

    public override void ApplySynchronizedUpdate(PropagationMessage ReceivedUpdate)
    {
        GPriorityQueueMsg<T> received = (GPriorityQueueMsg<T>)ReceivedUpdate;
        this.Merge(received);
    }

    public override PropagationMessage DecodePropagationMessage(byte[] input)
    {
        GPriorityQueueMsg<T> msg = new();
        msg.Decode(input);
        return msg;
    }

    public override PropagationMessage GetLastSynchronizedUpdate()
    {
        return new GPriorityQueueMsg<T>(this._priorityQueue);
    }
}
