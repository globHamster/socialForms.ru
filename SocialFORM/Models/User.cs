using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialFORM.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

        public int? RoleId { get; set; }
        public Role Role { get; set; }

        public ICollection<Privilege> Privileges { get; set; }
        public ICollection<DataUser> DataUsers { get; set; }
        public User()
        {
            Privileges = new List<Privilege>();
            DataUsers = new List<DataUser>();
        }
    }

    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}