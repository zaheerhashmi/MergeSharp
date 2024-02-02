using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using MergeSharp;

[TypeAntiEntropyProtocol(typeof(GMultiset<>))]
public class GMultisetMsg<T> : PropagationMessage
{
    [JsonInclude]
    public Dictionary<T, int> _multiset;

    public GMultisetMsg()
    {
    }

    public Dictionary<T, int> Multiset
    {
        get { return _multiset; }
    }

    public GMultisetMsg(Dictionary<T, int> multiset)
    {
        this._multiset = multiset;
    }

    public override void Decode(byte[] input)
    {
        var json = JsonSerializer.Deserialize<GMultisetMsg<T>>(input);
        this._multiset = json._multiset;
    }

    public override byte[] Encode()
    {
        return JsonSerializer.SerializeToUtf8Bytes(this);
    }
}

[ReplicatedType("GMultiset")]
public class GMultiset<T> : CRDT
{
    private Dictionary<T, int> _multiset;

    public GMultiset()
    {
        this._multiset = new Dictionary<T, int>();
    }

    [OperationType(OpType.Update)]
    public virtual void Add(T item)
    {
        if (!_multiset.ContainsKey(item))
        {
            _multiset[item] = 0;
        }
        _multiset[item]++;
    }

    public int Count(T item)
    {
        return _multiset.ContainsKey(item) ? _multiset[item] : 0;
    }

    public void Merge(GMultisetMsg<T> received)
    {
        foreach (var kv in received.Multiset) // Change '_elements' to 'Multiset'
        {
            this._multiset.TryGetValue(kv.Key, out int value);
            this._multiset[kv.Key] = value + kv.Value;
        }
    }

    public override void ApplySynchronizedUpdate(PropagationMessage ReceivedUpdate)
    {
        GMultisetMsg<T> received = (GMultisetMsg<T>)ReceivedUpdate;
        this.Merge(received);
    }

    public override PropagationMessage DecodePropagationMessage(byte[] input)
    {
        GMultisetMsg<T> msg = new();
        msg.Decode(input);
        return msg;
    }

    public override PropagationMessage GetLastSynchronizedUpdate()
    {
        return new GMultisetMsg<T>(this._multiset);
    }
}
