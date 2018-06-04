using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialFORM.Models.Session
{
    public class SessionHubModel
    {
        public int Id { get; set; }
        public string ConnectionId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Date { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string TimeInSystem { get; set; }
        public string AfkTime { get; set; }
        public Boolean IsAction { get; set; }
    }
}