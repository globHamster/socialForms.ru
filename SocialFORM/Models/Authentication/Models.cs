using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialFORM.Models.Authentication
{
    public class LoginModel
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
    public class RegisterModel
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public int Age { get; set; }
        public string Fool { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Family { get; set; }
        public int RoleId { get; set; }
    }
} 