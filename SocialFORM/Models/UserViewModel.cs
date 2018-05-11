using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialFORM.Models
{
    public class UserViewModel
    {
        public int IdView { get; set; }
        public string LoginView { get; set; }
        public string PasswordView { get; set; }
        public string NameView { get; set; }
        public string FamilyView { get; set; }
        public int AgeView { get; set; }
        public string FoolView { get; set; }
        public string EmailView { get; set; }
        public int PrivilegeView { get; set; }
        public string RoleView { get; set; }
        public int RoleIdView { get; set; }
        public bool? SchoolDayView { get; set; }
    }
}