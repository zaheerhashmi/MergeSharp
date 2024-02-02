using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MergeSharp
{
    [TypeAntiEntropyProtocol(typeof(LWWRegister<>))]
    public class LWWRegisterMsg<T> : PropagationMessage
    {
        [JsonInclude]
        public T Value;

        [JsonInclude]
        public long Timestamp;

        public LWWRegisterMsg()
        {
        }

        public LWWRegisterMsg(T value, long timestamp)
        {
            Value = value;
            Timestamp = timestamp;
        }

        public override void Decode(byte[] input)
        {
            var json = JsonSerializer.Deserialize<LWWRegisterMsg<T>>(input);
            Value = json.Value;
            Timestamp = json.Timestamp;
        }

        public override byte[] Encode()
        {
            return JsonSerializer.SerializeToUtf8Bytes(this);
        }
    }

    [ReplicatedType("LWWRegister")]
    public class LWWRegister<T> : CRDT
    {
        private T _value;
        private long _timestamp;

        public LWWRegister()
        {
            _value = default(T);
            _timestamp = 0;
        }

        [OperationType(OpType.Update)]
        public virtual void Assign(T value, long timestamp)
        {
            if (timestamp > _timestamp)
            {
                _value = value;
                _timestamp = timestamp;
            }
        }

        public T GetValue()
        {
            return _value;
        }

        public override void ApplySynchronizedUpdate(PropagationMessage receivedUpdate)
        {
            LWWRegisterMsg<T> received = (LWWRegisterMsg<T>)receivedUpdate;
            Assign(received.Value, received.Timestamp);
        }

        public override PropagationMessage DecodePropagationMessage(byte[] input)
        {
            LWWRegisterMsg<T> msg = new();
            msg.Decode(input);
            return msg;
        }

        public override PropagationMessage GetLastSynchronizedUpdate()
        {
            return new LWWRegisterMsg<T>(_value, _timestamp);
        }
    }
}
