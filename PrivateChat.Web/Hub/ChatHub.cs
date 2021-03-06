﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Core.Components;
using Dal.Repositories.IRepositories;
using Microsoft.AspNet.SignalR.Hubs;

namespace PrivateChat.Web.Hub
{
    [HubName("chatHub")]
    public class ChatHub : Microsoft.AspNet.SignalR.Hub //: PersistentConnection 
    {
        private static IDictionary<Guid, List<ConnectionInfo>> Connections { get; set; }

        public ChatHub() {
            if (Connections == null) {
                Connections = new Dictionary<Guid, List<ConnectionInfo>>();
            }
        }

        public void Register(Guid userId, Guid opponentId)
        {
            var conInfo = new ConnectionInfo { With = opponentId, Connection = Context.ConnectionId };

            if (Connections.ContainsKey(userId))
            {
                var userCons = Connections[userId];
                if (userCons != null)
                {
                    userCons.Add(conInfo);
                }
            }
            else
            {
                Connections.Add( userId, new List<ConnectionInfo> { conInfo });
            }
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var isFound = false;
            foreach (var userCons in Connections) {
                foreach (var con in userCons.Value) {
                    if (con.Connection == Context.ConnectionId) {
                        userCons.Value.Remove(con);
                        if (!userCons.Value.Any()) {
                            Connections.Remove(userCons.Key);
                        }
                        isFound = true;
                        break;
                    }
                }
                if (isFound) {
                    break;
                }
            }

            return base.OnDisconnected(stopCalled);
        }

        public void SaveMessage(string text, Guid authorId, Guid recipientId, string clientMessageId)
        {
            var messageRepository = IoC.Instance.Resolve<IMessageRepository>();
            var userRepository = IoC.Instance.Resolve<IUserRepository>();

            var users = userRepository.GetChatUsersByHashes(authorId, recipientId);

            var msg =  messageRepository.CreateMessage(users.Author.Id, users.Recipient.Id, text);

            if (!Connections.ContainsKey(authorId)) {
                return;
            }

            var connections = Connections[authorId].Select(x=>x.Connection).ToList();
            Clients.Clients(connections).MessageSaved(clientMessageId, msg.DisplayCreateDate, text);
            
            if (!Connections.ContainsKey(recipientId))
            {
                return;
            }
            var readerConnections = Connections[recipientId].Select(x => x.Connection).ToList();
            Clients.Clients(readerConnections).ReceiveMessage(text, false, msg.DisplayCreateDate, clientMessageId);
        }

        public void ReadMessages(Guid authorId, Guid recipientId)
        {
            var messageRepository = IoC.Instance.Resolve<IMessageRepository>();
            var userRepository = IoC.Instance.Resolve<IUserRepository>();
            var users = userRepository.GetChatUsersByHashes(authorId, recipientId);

            var msgs = messageRepository.MarkAsRead(users.Author.Id, users.Recipient.Id);

            if (!Connections.ContainsKey(recipientId)) {
                return; 
            }

            var connections = Connections[recipientId].Where(x=>x.With == authorId).Select(x=>x.Connection).ToList();

            if (connections == null || !connections.Any()) return;

            Clients.Clients(connections).RecipientReadMessages(msgs.ToList());
        }
    }

    public class ConnectionInfo {
        public Guid With { get; set; }
        public string Connection { get; set; }
    }
}