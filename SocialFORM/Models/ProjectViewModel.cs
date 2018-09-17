using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialFORM.Models
{
    public class ProjectViewModel
    {
        public int IDView { get; set; }
        public int ProjectIDView { get; set; }
        public int UserIDView { get; set; }
        public string UserNameView { get; set; }
        public string PhoneView { get; set; }
        public string DateView { get; set; }
        public string StartTimeView { get; set; }
        public string EndTimeView { get; set; }
        public string LenghtTimeView { get; set; }
    }
}