using System.Collections.Generic;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
namespace MergeSharp.UDPConnectionManager;

public class udpConnectionManager : IConnectionManager
{
	private UDPManagerServer managerServer;
	private List<UDPNode> nodes;
	public UDPNode selfNode { get; private set; }
	private readonly ILogger logger;

	public udpConnectionManager(List<UDPNode> nodes, UDPNode self, ILogger logger = null)
	{
		this.nodes = nodes;
		this.selfNode = self;
		this.managerServer = new UDPManagerServer(this.selfNode, this);
		this.logger = logger ?? NullLogger.Instance;
	}

	// This constructor takes the ip and port for self node, and a list of strings nodes' ip and port as input
	public udpConnectionManager(string ip, string port, IEnumerable<string> nodes)
	{
		this.nodes = new List<UDPNode>();
		foreach (string n in nodes)
		{
			string[] ipAndPort = n.Split(':');
			UDPNode node = new UDPNode(ipAndPort[0], int.Parse(ipAndPort[1]));

			// If ip and port equal to self
			if (ipAndPort[0] == ip && ipAndPort[1] == port)
			{
				node.isSelf = true;
				this.selfNode = node;
			}

			this.nodes.Add(node);
		}

		this.managerServer = new UDPManagerServer(this.selfNode, this);
	}

	public void Start()
	{
		managerServer.Run();
	}

	public void Stop()
	{
		managerServer.Stop();
	}

	~udpConnectionManager()
	{
		this.Dispose();
	}

	public void Dispose()
	{
		this.Stop();
	}

	public event EventHandler<SyncMsgEventArgs> ReplicationManagerSyncMsgHandlerEvent;

	public void ExecuteSyncMessage(NetworkProtocol msg)
	{
		ReplicationManagerSyncMsgHandlerEvent(this, new SyncMsgEventArgs(msg));
	}

	public void PropagateSyncMsg(NetworkProtocol msg)
	{
		foreach (UDPNode node in nodes)
		{
			if (node.isSelf)
			{
				continue;
			}
			node.Send(msg);
		}
	}
}
