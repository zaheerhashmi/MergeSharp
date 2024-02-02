using System;
using MergeSharp;


namespace MergeSharp
{

    public class DummyNode : ConnectionEndpoint
    {
        public int id;
        public DummyConnectionManager recivingConnectionManager;

        public DummyNode(int id)
        {
            this.id = id;
        }

    

    }


    // used for testing
    public class DummyConnectionManager : IConnectionManager
    {
        
        public DummyNode[] nodes;

        public int nodeIdx;

        public event EventHandler<SyncMsgEventArgs> ReplicationManagerSyncMsgHandlerEvent;
        public event ReceivedSyncEventHandler RecivedSyncEvent;

        NetworkProtocol recievedBuffer;
        public DummyConnectionManager(DummyNode[] nodes, int replicaIndex)
        {
            this.nodes = nodes;
            this.nodeIdx = replicaIndex;
        }

        public void BroadCast(NetworkProtocol msg)
        {
            foreach (var n in this.nodes)
            {   
                if (n.id != this.nodeIdx)
                    this.Send(msg, n);
            }

        }

        public int NumMembers()
        {
            return this.nodes.Length;
        }


        public void RegisterCRDTRequestHandler(EventHandler<SyncMsgEventArgs> handler)
        {
            this.ReplicationManagerSyncMsgHandlerEvent += handler;
        }

        public void Send(NetworkProtocol msg, ConnectionEndpoint reciever)
        {
            DummyNode node = (DummyNode) reciever;
            node.recivingConnectionManager.Recieve(msg);
        }



        public void Recieve(NetworkProtocol msg)
        {
            this.recievedBuffer = msg;
            this.RecievedSyncMsg();
        }



        public void PropagateSyncMsg(NetworkProtocol msg)
        {
            this.BroadCast(msg);
        }

        public void RecievedSyncMsg()
        {
            SyncMsgEventArgs args = new SyncMsgEventArgs(this.recievedBuffer);
            this.ReplicationManagerSyncMsgHandlerEvent(this, args);
        }

        public int CurMemberPosition()
        {
            return this.nodeIdx;
        }

        public void Dispose()
        {
            return;
        }

        public int[] AllMemberIds()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            return;
        }

        public void Stop()
        {
            return;
        }
    }




}
