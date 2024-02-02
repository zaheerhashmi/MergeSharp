using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using MergeSharp;

[TypeAntiEntropyProtocol(typeof(GList<>))]
public class GListMsg<T> : PropagationMessage
{
    [JsonInclude]
    public List<T> elements;

    public GListMsg()
    {
    }

    public GListMsg(List<T> elements)
    {
        this.elements = elements;
    }

    public override void Decode(byte[] input)
    {
        var json = JsonSerializer.Deserialize<GListMsg<T>>(input);
        this.elements = json.elements;
    }

    public override byte[] Encode()
    {
        return JsonSerializer.SerializeToUtf8Bytes(this);
    }
}

[ReplicatedType("GList")]
public class GList<T> : CRDT
{
    private List<T> elements;

    public GList()
    {
        this.elements = new List<T>();
    }

    [OperationType(OpType.Update)]
    public virtual void Add(T item)
    {
        this.elements.Add(item);
    }

    public List<T> LookupAll()
    {
        return this.elements;
    }

    public void Merge(GListMsg<T> received)
    {
        this.elements = this.elements.Union(received.elements).ToList();
    }

    public override void ApplySynchronizedUpdate(PropagationMessage ReceivedUpdate)
    {
        GListMsg<T> received = (GListMsg<T>)ReceivedUpdate;
        this.Merge(received);
    }

    public override PropagationMessage DecodePropagationMessage(byte[] input)
    {
        GListMsg<T> msg = new();
        msg.Decode(input);
        return msg;
    }

    public override PropagationMessage GetLastSynchronizedUpdate()
    {
        return new GListMsg<T>(this.elements);
    }
}
