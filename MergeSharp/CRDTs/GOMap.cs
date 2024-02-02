using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MergeSharp
{
    [TypeAntiEntropyProtocol(typeof(GrowOnlyMap<,>))]
    public class GrowOnlyMapMsg<TKey, TValue> : PropagationMessage
    {
        [JsonInclude]
        public Dictionary<TKey, TValue> Map;

        public GrowOnlyMapMsg()
        {
        }

        public GrowOnlyMapMsg(Dictionary<TKey, TValue> map)
        {
            Map = map;
        }

        public override void Decode(byte[] input)
        {
            var json = JsonSerializer.Deserialize<GrowOnlyMapMsg<TKey, TValue>>(input);
            Map = json.Map;
        }

        public override byte[] Encode()
        {
            return JsonSerializer.SerializeToUtf8Bytes(this);
        }
    }

    [ReplicatedType("GrowOnlyMap")]
    public class GrowOnlyMap<TKey, TValue> : CRDT
    {
        private Dictionary<TKey, TValue> _map;

        public GrowOnlyMap()
        {
            _map = new Dictionary<TKey, TValue>();
        }

        [OperationType(OpType.Update)]
        public virtual void Put(TKey key, TValue value)
        {
            if (!_map.ContainsKey(key))
            {
                _map[key] = value;
            }
        }

        public TValue Get(TKey key)
        {
            if (_map.TryGetValue(key, out TValue value))
            {
                return value;
            }
            return default(TValue);
        }

        public override void ApplySynchronizedUpdate(PropagationMessage receivedUpdate)
        {
            GrowOnlyMapMsg<TKey, TValue> received = (GrowOnlyMapMsg<TKey, TValue>)receivedUpdate;
            Merge(received);
        }

        public void Merge(GrowOnlyMapMsg<TKey, TValue> received)
        {
            foreach (var kv in received.Map)
            {
                Put(kv.Key, kv.Value);
            }
        }

        public override PropagationMessage DecodePropagationMessage(byte[] input)
        {
            GrowOnlyMapMsg<TKey, TValue> msg = new();
            msg.Decode(input);
            return msg;
        }

        public override PropagationMessage GetLastSynchronizedUpdate()
        {
            return new GrowOnlyMapMsg<TKey, TValue>(_map);
        }
    }
}
