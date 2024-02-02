using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using MergeSharp;

[TypeAntiEntropyProtocol(typeof(GSet<>))]
public class GSetMsg<T> : PropagationMessage
{
    [JsonInclude]
    public HashSet<T> set;

    public GSetMsg()
    {
    }

    public GSetMsg(HashSet<T> set)
    {
        this.set = set;
    }

    public override void Decode(byte[] input)
    {
        var json = JsonSerializer.Deserialize<GSetMsg<T>>(input);
        this.set = json.set;
    }

    public override byte[] Encode()
    {
        return JsonSerializer.SerializeToUtf8Bytes(this);
    }
}

[ReplicatedType("GSet")]
public class GSet<T> : CRDT, ICollection<T>
{
    private HashSet<T> _set;

    public int Count => _set.Count;

    public bool IsReadOnly => false;

    public GSet()
    {
        this._set = new HashSet<T>();
    }

    [OperationType(OpType.Update)]
    public virtual void Add(T item)
    {
        this._set.Add(item);
    }

    public void Merge(GSetMsg<T> received)
    {
        this._set.UnionWith(received.set);
    }

    public override void ApplySynchronizedUpdate(PropagationMessage ReceivedUpdate)
    {
        GSetMsg<T> received = (GSetMsg<T>)ReceivedUpdate;
        this.Merge(received);
    }

    public override PropagationMessage DecodePropagationMessage(byte[] input)
    {
        GSetMsg<T> msg = new();
        msg.Decode(input);
        return msg;
    }

    public override PropagationMessage GetLastSynchronizedUpdate()
    {
        return new GSetMsg<T>(this._set);
    }

    public bool Contains(T item)
    {
        return this._set.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        this._set.CopyTo(array, arrayIndex);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return this._set.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Remove(T item)
    {
        throw new InvalidOperationException("Cannot remove an item from a G-Set");
    }

    public void Clear()
    {
        throw new InvalidOperationException("Cannot clear a G-Set");
    }
}
