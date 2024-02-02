using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

namespace MergeSharp
{
    public class MStringIdentifier : IComparable<MStringIdentifier>
    {
        public int Position;
        public Guid SiteId;

        public MStringIdentifier(int position, Guid siteId)
        {
            Position = position;
            SiteId = siteId;
        }

        public int CompareTo(MStringIdentifier other)
        {
            int posComparison = Position.CompareTo(other.Position);
            if (posComparison != 0)
            {
                return posComparison;
            }

            return SiteId.CompareTo(other.SiteId);
        }
    }

    public class MStringEntry
    {
        public MStringIdentifier Id;
        public char Value;

        public MStringEntry(MStringIdentifier id, char value)
        {
            Id = id;
            Value = value;
        }
    }

    [TypeAntiEntropyProtocol(typeof(MString))]
    public class MStringMsg : PropagationMessage
    {
        [JsonInclude]
        public List<MStringEntry> Entries;

        public MStringMsg()
        {
        }

        public MStringMsg(List<MStringEntry> entries)
        {
            Entries = entries;
        }

        public override void Decode(byte[] input)
        {
            var json = JsonSerializer.Deserialize<MStringMsg>(input);
            Entries = json.Entries;
        }

        public override byte[] Encode()
        {
            return JsonSerializer.SerializeToUtf8Bytes(this);
        }
    }

    [ReplicatedType("MString")]
    public class MString : CRDT
    {
        private readonly Guid _siteId;
        private List<MStringEntry> _entries;

        public MString()
        {
            _siteId = Guid.NewGuid();
            _entries = new List<MStringEntry>();
        }

        [OperationType(OpType.Update)]
        public void Insert(int index, char value)
        {
            MStringIdentifier identifier = GenerateIdentifier(index);
            MStringEntry entry = new MStringEntry(identifier, value);
            _entries.Insert(index, entry);
        }

        [OperationType(OpType.Update)]
        public void Delete(int index)
        {
            _entries.RemoveAt(index);
        }

        public string GetText()
        {
            return new string(_entries.Select(e => e.Value).ToArray());
        }

        public override void ApplySynchronizedUpdate(PropagationMessage receivedUpdate)
        {
            MStringMsg received = (MStringMsg)receivedUpdate;
            Merge(received);
        }

        public void Merge(MStringMsg received)
        {
            List<MStringEntry> merged = new List<MStringEntry>(_entries.Count + received.Entries.Count);

            int localIndex = 0;
            int receivedIndex = 0;

            while (localIndex < _entries.Count && receivedIndex < received.Entries.Count)
            {
                MStringEntry localEntry = _entries[localIndex];
                MStringEntry receivedEntry = received.Entries[receivedIndex];

                if (localEntry.Id.CompareTo(receivedEntry.Id) < 0)
                {
                    merged.Add(localEntry);
                    localIndex++;
                }
                else
                {
                    merged.Add(receivedEntry);
                    receivedIndex++;
                }
            }

            while (localIndex < _entries.Count)
            {
                merged.Add(_entries[localIndex]);
                localIndex++;
            }

            while (receivedIndex < received.Entries.Count)
            {
                merged.Add(received.Entries[receivedIndex]);
                receivedIndex++;
            }

            _entries = merged;
        }

        public override PropagationMessage DecodePropagationMessage(byte[] input)
        {
            MStringMsg msg = new();
            msg.Decode(input);
            return msg;
        }
        public override PropagationMessage GetLastSynchronizedUpdate()
        {
            return new MStringMsg(_entries);
        }

        private MStringIdentifier GenerateIdentifier(int index)
        {
            int position;
            if (index == 0)
            {
                position = 0;
            }
            else
            {
                MStringIdentifier prevIdentifier = _entries[index - 1].Id;
                MStringIdentifier nextIdentifier = index < _entries.Count ? _entries[index].Id : null;

                if (nextIdentifier == null || prevIdentifier.Position + 1 < nextIdentifier.Position)
                {
                    position = prevIdentifier.Position + 1;
                }
                else
                {
                    position = prevIdentifier.Position + 2;
                }
            }

            return new MStringIdentifier(position, _siteId);
        }
    }
}