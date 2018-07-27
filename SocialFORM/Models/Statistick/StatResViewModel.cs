using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialFORM.Models.Statistick
{
    public class StatResViewModel
    {
        public int IdView { get; set; }
        public string DataView { get; set; }
        public string CountUserView { get; set; }
        public string CountProjectView { get; set; }
        public string CountHourView { get; set; }
        public string MediumTimeView { get; set; }
        public string MinLenghtView { get; set; }
        public string MaxLenghtView { get; set; }
        public string MediumView { get; set; }
        public string OneNView { get; set; }
        public string TimeUpView { get; set; }
        public string TimeOutView { get; set; }
        public string TimeWorkView { get; set; }
        public string TimeAfkView { get; set; }
    }
}