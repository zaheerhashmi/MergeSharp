using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MergeSharp
{
    [TypeAntiEntropyProtocol(typeof(LWWMultiSet<>))]
    public class LWWMultiSetMsg<T> : PropagationMessage
    {
        [JsonInclude]
        public Dictionary<T, long> addSet;
        [JsonInclude]
        public Dictionary<T, long> removeSet;

        public LWWMultiSetMsg()
        {
        }

        public LWWMultiSetMsg(Dictionary<T, long> addSet, Dictionary<T, long> removeSet)
        {
            this.addSet = addSet;
            this.removeSet = removeSet;
        }

        public override void Decode(byte[] input)
        {
            var json = JsonSerializer.Deserialize<LWWMultiSetMsg<T>>(input);
            this.addSet = json.addSet;
            this.removeSet = json.removeSet;
        }

        public override byte[] Encode()
        {
            return JsonSerializer.SerializeToUtf8Bytes(this);
        }
    }

    [ReplicatedType("LWWMultiSet")]
    public class LWWMultiSet<T> : CRDT
    {
        private Dictionary<T, long> addSet;
        private Dictionary<T, long> removeSet;

        public LWWMultiSet()
        {
            this.addSet = new Dictionary<T, long>();
            this.removeSet = new Dictionary<T, long>();
        }

        [OperationType(OpType.Update)]
        public virtual void Add(T item, long timestamp)
        {
            if (addSet.TryGetValue(item, out long existingTimestamp))
            {
                if (timestamp > existingTimestamp)
                {
                    addSet[item] = timestamp;
                }
            }
            else
            {
                addSet[item] = timestamp;
            }
        }

        [OperationType(OpType.Update)]
        public virtual bool Remove(T item, long timestamp)
        {
            if (Contains(item))
            {
                if (removeSet.TryGetValue(item, out long existingTimestamp))
                {
                    if (timestamp > existingTimestamp)
                    {
                        removeSet[item] = timestamp;
                    }
                }
                else
                {
                    removeSet[item] = timestamp;
                }

                return true;
            }
            return false;
        }

        public bool Contains(T item)
        {
            if (addSet.TryGetValue(item, out long addTimestamp) &&
                (!removeSet.TryGetValue(item, out long removeTimestamp) || removeTimestamp < addTimestamp))
            {
                return true;
            }
            return false;
        }

        public void Merge(LWWMultiSetMsg<T> received)
        {
            foreach (var entry in received.addSet)
            {
                Add(entry.Key, entry.Value);
            }

            foreach (var entry in received.removeSet)
            {
                if (addSet.TryGetValue(entry.Key, out long addTimestamp) && entry.Value > addTimestamp)
                {
                    Remove(entry.Key, entry.Value);
                }
            }
        }

        public override void ApplySynchronizedUpdate(PropagationMessage ReceivedUpdate)
        {
            LWWMultiSetMsg<T> received = (LWWMultiSetMsg<T>)ReceivedUpdate;
            this.Merge(received);
        }

        public override PropagationMessage DecodePropagationMessage(byte[] input)
        {
            LWWMultiSetMsg<T> msg = new();
            msg.Decode(input);
            return msg;
        }

        public override PropagationMessage GetLastSynchronizedUpdate()
        {
            return new LWWMultiSetMsg<T>(this.addSet, this.removeSet);
        }
    }
}
