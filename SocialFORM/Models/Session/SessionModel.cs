using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialFORM.Models.Session
{
    public class SessionModel
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public DateTime Date { get; set; }
        public string TimeUp { get; set; }
        public string SetTimeUp { get; set; }
        public string TimeOut { get; set; }
        public string AllTime { get; set; }
        public int StatusTime { get; set; }
    }
}