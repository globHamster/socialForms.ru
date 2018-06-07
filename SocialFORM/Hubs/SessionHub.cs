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
            Clients.Caller.onConnected(id, userName, time);

            // Посылаем сообщение всем пользователям, кроме текущего
            string DateNow = DateTime.Now.ToShortDateString();
            List<SessionHubModel> sessionHubModels_tmp = context.SetSessionHubModel.Where(u => u.Date == DateNow).ToList();
            List<SessionHubModel> result = sessionHubModels_tmp.Where(u => u.IsAction == true || (u.IsAction == false && u.EndTime == null)).ToList();
            Clients.AllExcept(id).onNewUserConnected(result);

        }

        public void Monitoring()
        {
            string DateNow = DateTime.Now.ToShortDateString();
            List<SessionHubModel> sessionHubModels_tmp = context.SetSessionHubModel.Where(u => u.Date == DateNow).ToList();
            List<SessionHubModel> result = sessionHubModels_tmp.Where(u => u.IsAction == true || (u.IsAction == false && u.EndTime == null)).ToList();
            System.Diagnostics.Debug.WriteLine("Monitoring COUNT================>>>>>" + result.Count);
            Clients.Caller.onMonitoring(result);
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
            string DateNow = DateTime.Now.ToShortDateString();
            List<SessionHubModel> sessionHubModels_tmp = context.SetSessionHubModel.Where(u => u.Date == DateNow).ToList();
            List<SessionHubModel> result = sessionHubModels_tmp.Where(u => u.IsAction == true || (u.IsAction == false && u.EndTime == null)).ToList();
            Clients.All.fsAFK(result);
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
            string DateNow = DateTime.Now.ToShortDateString();
            List<SessionHubModel> sessionHubModels_tmp = context.SetSessionHubModel.Where(u => u.Date == DateNow).ToList();
            List<SessionHubModel> result = sessionHubModels_tmp.Where(u => u.IsAction == true || (u.IsAction == false && u.EndTime == null)).ToList();
            Clients.All.feAFK(result);
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
