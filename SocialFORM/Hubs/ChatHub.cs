using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SocialFORM.Models;
using Microsoft.AspNet.SignalR;

namespace SocialFORM.Hubs
{
    public class ChatHub : Hub
    {
        static List<ChatUser> UsersChat = new List<ChatUser>();



        // Подключение нового пользователя
        public void Connect(string userName)
        {
            var id = Context.ConnectionId;


            if (!UsersChat.Any(x => x.ConnectionId == id))
            {
                UsersChat.Add(new ChatUser { ConnectionId = id, Name = userName });

                // Посылаем сообщение текущему пользователю
                Clients.Caller.onConnected(id, userName, UsersChat);

                // Посылаем сообщение всем пользователям, кроме текущего
                Clients.AllExcept(id).onNewUserConnected(id, userName);
            }
        }

        // Отключение пользователя
        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            var item = UsersChat.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (item != null)
            {
                UsersChat.Remove(item);
                var id = Context.ConnectionId;
                Clients.All.onUserDisconnected(id, item.Name);
            }

            return base.OnDisconnected(stopCalled);
        }
    }
}