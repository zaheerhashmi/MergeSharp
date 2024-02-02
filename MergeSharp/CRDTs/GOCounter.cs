using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using MergeSharp;

[TypeAntiEntropyProtocol(typeof(GCounter))]
public class GCounterMsg : PropagationMessage
{
	[JsonInclude]
	public Dictionary<Guid, int> vector;

	public GCounterMsg()
	{
	}

	public GCounterMsg(Dictionary<Guid, int> vector)
	{
		this.vector = vector;
	}

	public override void Decode(byte[] input)
	{
		var json = JsonSerializer.Deserialize<GCounterMsg>(input);
		this.vector = json.vector;
	}

	public override byte[] Encode()
	{
		return JsonSerializer.SerializeToUtf8Bytes(this);
	}
}

[ReplicatedType("GCounter")]
public class GCounter : CRDT
{
	private Dictionary<Guid, int> _vector;
	private Guid replicaIdx;

	public GCounter()
	{
		this.replicaIdx = Guid.NewGuid();
		this._vector = new Dictionary<Guid, int>();
		this._vector[this.replicaIdx] = 0;
	}

	public int Get()
	{
		return this._vector.Sum(x => x.Value);
	}

	[OperationType(OpType.Update)]
	public virtual void Increment(int i)
	{
		this._vector[this.replicaIdx] += i;
	}

	public void Merge(GCounterMsg received)
	{
		foreach (var kv in received.vector)
		{
			this._vector.TryGetValue(kv.Key, out int value);
			this._vector[kv.Key] = Math.Max(value, kv.Value);
		}
	}

	public override void ApplySynchronizedUpdate(PropagationMessage ReceivedUpdate)
	{
		GCounterMsg received = (GCounterMsg)ReceivedUpdate;
		this.Merge(received);
	}

	public override PropagationMessage DecodePropagationMessage(byte[] input)
	{
		GCounterMsg msg = new();
		msg.Decode(input);
		return msg;
	}

	public override PropagationMessage GetLastSynchronizedUpdate()
	{
		return new GCounterMsg(this._vector);
	}
}
