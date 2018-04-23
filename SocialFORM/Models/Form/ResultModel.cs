using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialFORM.Models.Form
{
    public class ResultModel
    {
        public int Id { get; set; }
        public int ProjectID { get; set; }
        public int UserID { get; set; }
        public int BlankID { get; set; }
        public int ResultIndex { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime Data { get; set; }
        public string Time { get; set; }
        public string CoordWidth { get; set; }
        public string CoordHeight { get; set; }
        public string UserName { get; set; }
    }
}