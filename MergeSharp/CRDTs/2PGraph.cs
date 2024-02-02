using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MergeSharp
{
    [TypeAntiEntropyProtocol(typeof(TwoPhaseGraph<>))]
    public class TwoPhaseGraphMsg<T> : PropagationMessage
    {
        [JsonInclude]
        public HashSet<T> addVertices;
        [JsonInclude]
        public HashSet<T> removeVertices;
        [JsonInclude]
        public HashSet<(T, T)> addEdges;
        [JsonInclude]
        public HashSet<(T, T)> removeEdges;

        public TwoPhaseGraphMsg()
        {
        }

        public TwoPhaseGraphMsg(HashSet<T> addVertices, HashSet<T> removeVertices, HashSet<(T, T)> addEdges, HashSet<(T, T)> removeEdges)
        {
            this.addVertices = addVertices;
            this.removeVertices = removeVertices;
            this.addEdges = addEdges;
            this.removeEdges = removeEdges;
        }

        public override void Decode(byte[] input)
        {
            var json = JsonSerializer.Deserialize<TwoPhaseGraphMsg<T>>(input);
            this.addVertices = json.addVertices;
            this.removeVertices = json.removeVertices;
            this.addEdges = json.addEdges;
            this.removeEdges = json.removeEdges;
        }

        public override byte[] Encode()
        {
            return JsonSerializer.SerializeToUtf8Bytes(this);
        }
    }

    [ReplicatedType("TwoPhaseGraph")]
    public class TwoPhaseGraph<T> : CRDT
    {
        private HashSet<T> addVertices;
        private HashSet<T> removeVertices;
        private HashSet<(T, T)> addEdges;
        private HashSet<(T, T)> removeEdges;

        public TwoPhaseGraph()
        {
            this.addVertices = new HashSet<T>();
            this.removeVertices = new HashSet<T>();
            this.addEdges = new HashSet<(T, T)>();
            this.removeEdges = new HashSet<(T, T)>();
        }

        [OperationType(OpType.Update)]
        public virtual void AddVertex(T vertex)
        {
            this.addVertices.Add(vertex);
        }

        [OperationType(OpType.Update)]
        public virtual void RemoveVertex(T vertex)
        {
            if (this.addVertices.Contains(vertex))
            {
                this.removeVertices.Add(vertex);
            }
        }

        [OperationType(OpType.Update)]
        public virtual void AddEdge(T from, T to)
        {
            if (this.addVertices.Contains(from) && this.addVertices.Contains(to) && !this.removeVertices.Contains(from) && !this.removeVertices.Contains(to))
            {
                this.addEdges.Add((from, to));
            }
        }

        [OperationType(OpType.Update)]
        public virtual void RemoveEdge(T from, T to)
        {
            var edge = (from, to);
            if (this.addEdges.Contains(edge))
            {
                this.removeEdges.Add(edge);
            }
        }

        public bool ContainsVertex(T vertex)
        {
            return this.addVertices.Contains(vertex) && !this.removeVertices.Contains(vertex);
        }

        public bool ContainsEdge(T from, T to)
        {
            var edge = (from, to);
            return this.addEdges.Contains(edge) && !this.removeEdges.Contains(edge);
        }

        public void Merge(TwoPhaseGraphMsg<T> received)
        {
            this.addVertices.UnionWith(received.addVertices);
            this.removeVertices.UnionWith(received.removeVertices);
            this.addEdges.UnionWith(received.addEdges);
            this.removeEdges.UnionWith(received.removeEdges);
        }

        public override void ApplySynchronizedUpdate(PropagationMessage receivedUpdate)
        {
            TwoPhaseGraphMsg<T> received = (TwoPhaseGraphMsg<T>)receivedUpdate;
            this.Merge(received);
        }

        public override PropagationMessage DecodePropagationMessage(byte[] input)
        {
            TwoPhaseGraphMsg<T> msg = new();
            msg.Decode(input);
            return msg;
        }

        public override PropagationMessage GetLastSynchronizedUpdate()
        {
            return new TwoPhaseGraphMsg<T>(this.addVertices, this.removeVertices, this.addEdges, this.removeEdges);
        }
    }
}
