using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialFORM.Models.Project
{
    public class ProjectModel
    {
        public int Id { get; set; }
        public string NameProject { get; set; }
        public bool? ActionProject { get; set; }
        public string SettingEncode { get; set; }
    }
}