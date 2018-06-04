using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Web;
using Microsoft.AspNet.SignalR;
using SocialFORM.Models.Session;

namespace SocialFORM.Hubs
{
    public class SessionHub : Hub
    {
        static List<SessionHubModel> sessionHubs = new List<SessionHubModel>();
        Models.ApplicationContext context = new Models.ApplicationContext();

        // Подключение нового пользователя
        public void Connect(string userid ,string userName)
        {
            var id = Context.ConnectionId;
            var time = TimeSpan.Parse(DateTime.Now.ToLongTimeString());
            var date_tmp = DateTime.Now.ToShortDateString();

            //
            // Создаем в БД запись
            //System.Diagnostics.Debug.WriteLine("==========>>>>>Пользователь Авторизировался<<<<<============");
            context.SetSessionHubModel.Add(new SessionHubModel
            {
                ConnectionId = id,
                UserId = Convert.ToInt32(userid),
                UserName = userName,
                Date = DateTime.Now.ToShortDateString(),
                StartTime = DateTime.Now.ToLongTimeString(),
                IsAction = true
            });
            context.SaveChanges();
                        // Посылаем сообщение текущему пользователю
            Clients.Caller.onConnected(id, userName, time, sessionHubs);

            // Посылаем сообщение всем пользователям, кроме текущего
            Clients.AllExcept(id).onNewUserConnected(id, userName, time);

        }

        public void Startafk()
        {
            string sAFK = DateTime.Now.ToLongTimeString();
            var date_tmp = DateTime.Now.ToShortDateString();
            string connectionId = Context.ConnectionId;

            if (context.SetSessionHubModel.Any(x => x.ConnectionId == connectionId))
            {
                //Изменяем записи в бд при отключение
                SessionHubModel UPSetTimeUp = context.SetSessionHubModel.Where(u => u.ConnectionId == connectionId && u.Date == date_tmp).First();
                UPSetTimeUp.IsAction = false;
                context.Entry(UPSetTimeUp).State = EntityState.Modified;
                context.SaveChanges();
            }
            Clients.Caller.fsAFK(sAFK);
        }
        public void Endafk(string sAFK_tmp)
        {
            string eAFK = DateTime.Now.ToLongTimeString();
            var date_tmp = DateTime.Now.ToShortDateString();
            string connectionId = Context.ConnectionId;

            if (context.SetSessionHubModel.Any(x => x.ConnectionId == connectionId))
            {
                //Изменяем записи в бд при отключение
                SessionHubModel UPSetTimeUp = context.SetSessionHubModel.Where(u => u.ConnectionId == connectionId && u.Date == date_tmp).First();
                if (UPSetTimeUp.AfkTime == null)
                {
                    UPSetTimeUp.AfkTime = (TimeSpan.Parse(eAFK) - TimeSpan.Parse(sAFK_tmp)).ToString();
                }
                else
                {
                    UPSetTimeUp.AfkTime = (TimeSpan.Parse(UPSetTimeUp.AfkTime) + (TimeSpan.Parse(eAFK) - TimeSpan.Parse(sAFK_tmp))).ToString();
                }
                UPSetTimeUp.IsAction = true;
                context.Entry(UPSetTimeUp).State = EntityState.Modified;
                context.SaveChanges();
            }
        }


        // Отключение пользователя
        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            var date_tmp = DateTime.Now.ToShortDateString();
            string connectionId = Context.ConnectionId;

            if (context.SetSessionHubModel.Any(x => x.ConnectionId == connectionId))
            {
                //Изменяем записи в бд при отключение
                SessionHubModel UPSetTimeUp = context.SetSessionHubModel.Where(u => u.ConnectionId == connectionId && u.Date == date_tmp).First();
                UPSetTimeUp.EndTime = DateTime.Now.ToLongTimeString();
                UPSetTimeUp.TimeInSystem = (TimeSpan.Parse(DateTime.Now.ToLongTimeString()) - TimeSpan.Parse(UPSetTimeUp.StartTime)).ToString();
                UPSetTimeUp.IsAction = false;
                context.Entry(UPSetTimeUp).State = EntityState.Modified;
                context.SaveChanges();

                var item = context.SetSessionHubModel.FirstOrDefault(x => x.ConnectionId == connectionId);
                Clients.All.onUserDisconnected(connectionId, item.UserName);

            }

            return base.OnDisconnected(stopCalled);
        }

    }
}
