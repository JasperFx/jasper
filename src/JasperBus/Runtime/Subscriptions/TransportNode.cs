﻿using System;
using System.Linq;
using JasperBus.Configuration;

namespace JasperBus.Runtime.Subscriptions
{
    public class TransportNode
    {
        //For json deserialization
        public TransportNode()
        {
        }

        public TransportNode(ChannelGraph graph, string machineName)
        {
            NodeName = graph.Name;
            Address = graph.ControlChannel?.Uri ?? graph.FirstOrDefault(x => x.Incoming)?.Uri;
            MachineName = machineName;
            Id = $"{NodeName}@{machineName}";
        }

        public string NodeName { get; set; }
        public string Id { get; set; }
        public string MachineName { get; set; }
        public Uri Address { get; set; }

        public override string ToString()
        {
            return $"Id: {Id}, MachineName: {MachineName}, NodeName: {NodeName}";
        }
    }
}
